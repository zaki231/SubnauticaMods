﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		public static partial class Components
		{
			public static class SliderValue
			{
				interface IConfigFieldInfo { void setConfigField(Config.Field cfgField); }

				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					readonly Type valueCmpType;
					readonly string valueFormat;
					ModOption parentOption;

					public void init(ModOption option)
					{
						Debug.assert(option is SliderOption, "SliderValue.Add: option is not a slider");
						parentOption = option;
					}

					public Add(Type valueCmpType, string valueFormat)
					{
						this.valueCmpType = valueCmpType;
						this.valueFormat  = valueFormat;

						Debug.assert(typeof(ModSliderOption.SliderValue).IsAssignableFrom(valueCmpType),
							$"Custom value type {valueCmpType} is not derived from ModSliderOption.SliderValue");
					}

					public void handle(GameObject gameObject)
					{
						GameObject slider = gameObject.transform.Find("Slider").gameObject;
#if GAME_BZ
						slider.destroyComponent<ModSliderOption.SliderValue>(); // SMLHelper for BZ adds this component for any slider :(
#endif
						Component valueType = slider.AddComponent(valueCmpType);
						(valueType as ModSliderOption.SliderValue).ValueFormat = valueFormat;
						(valueType as IConfigFieldInfo)?.setConfigField(parentOption.cfgField);
					}
				}


				// slider value will be saved exactly as formatted for display
				public class ExactlyFormatted: ModSliderOption.SliderValue
				{
					public override float ConvertToDisplayValue(float value) => valueFormat.format(value).convert<float>();
				}

				// displayed value is percent of the field's range (not slider's range)
				// will not change saved value
				// don't check for incorrect ranges
				// uses reflection to avoid referencing 'UnityEngine.UI' in shared project
				public class RangePercent: ModSliderOption.SliderValue, IConfigFieldInfo
				{
					float min, max;

					public void setConfigField(Config.Field cfgField)
					{
						var range = cfgField.getAttr<Config.Field.RangeAttribute>();
						Debug.assert(range != null);

						min = range.min;
						max = range.max;
					}

					static readonly PropertyWrapper sliderValue = Type.GetType("UnityEngine.UI.Slider, UnityEngine.UI").property("value").wrap();
					static readonly PropertyWrapper text =
						Type.GetType(Mod.Consts.isGameSN? "UnityEngine.UI.Text, UnityEngine.UI": "TMPro.TextMeshProUGUI, Unity.TextMeshPro").property("text").wrap();

					object _slider, _label;

					protected override void UpdateLabel()
					{
						_slider ??= this.getFieldValue("slider");
						_label  ??= this.getFieldValue("label");

						float res = (sliderValue.get<float>(_slider) - min) / (max - min);
						text.set(_label, "{0:P0}".format(res));
					}

					public override float ValueWidth => 95f;
				}

				// breaks slider to several linear intervals
				public class Nonlinear: ModSliderOption.SliderValue
				{
					readonly List<(float, float)> sliderToDisplay = new List<(float, float)>();
					readonly List<(float, float)> displayToSlider = new List<(float, float)>();

					readonly List<(float sliderValue, string formatBefore, string formatAfter)> valueFormats = new List<(float, string, string)>();

					public override float ValueWidth => 0f;

					protected override IEnumerator Start() => null;

					protected override void InitConverters()
					{
						addValueInfo(0f, minValue);
						addValueInfo(1f, maxValue);

						sliderToDisplay.Sort((x, y) => Math.Sign(x.Item1 - y.Item1));
						displayToSlider.Sort((x, y) => Math.Sign(x.Item1 - y.Item1));
						valueFormats.Sort((x, y) => Math.Sign(x.sliderValue - y.sliderValue));
					}

					public override float ConvertToSliderValue(float value)
					{
						return convertValue(value, displayToSlider);
					}

					public override float ConvertToDisplayValue(float value)
					{
						int index = valueFormats.FindIndex(1, info => value <= info.sliderValue);
						if (index > 0)
							ValueFormat = valueFormats[index].formatBefore ?? valueFormats[index - 1].formatAfter ?? ValueFormat;

						return convertValue(value, sliderToDisplay);
					}

					float convertValue(float value, List<(float, float)> valueInfo)
					{
						float _convert(float fromLeft, float fromRight, float toLeft, float toRight) =>
							toLeft + (toRight - toLeft) / (fromRight - fromLeft) * (value - fromLeft); // zero ?

						int index = valueInfo.FindIndex(1, info => value <= info.Item1);
						return index < 1? value: _convert(valueInfo[index-1].Item1, valueInfo[index].Item1, valueInfo[index-1].Item2, valueInfo[index].Item2);
					}

					// conversion point, sliderValue <-> displayValue
					// formatBefore: value format for items lesser than sliderValue; formatAfter: for items greater than sliderValue
					protected void addValueInfo(float sliderValue, float displayValue, string formatBefore = null, string formatAfter = null)
					{
						sliderToDisplay.Add((sliderValue, displayValue));
						displayToSlider.Add((displayValue, sliderValue));

						valueFormats.Add((sliderValue, formatBefore, formatAfter));
					}
				}
			}
		}
	}
}