using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DiscordStatusGUI.Libs
{
    public class PathAnimate
    {
        public static Storyboard CreateAnimation(string FromData, string ToData, DoubleAnimation prototype, Path path)
        {
            FromData = FromData.Replace(",", " ").Replace("-", " -");
            ToData = ToData.Replace(",", " ").Replace("-", " -");

            var FromDataArr = FromData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var ToDataArr = ToData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (FromDataArr.Length != ToDataArr.Length)
                throw new Exception("Unequal number of figures");

            Storyboard storyboard = new Storyboard();
            
            void SetDataValue(int i, string newValue)
            {
                FromDataArr[i] = newValue;
                var s = PathGeometry.Parse(string.Join(" ", FromDataArr));
                path.Data = s;
            }

            for (var i = 0; i < FromDataArr.Length; i++)
            {
                foreach (var ch in FromDataArr[i])
                    if (Array.IndexOf(Element.ValueChars, ch) != -1)
                        goto begin;
                goto end;

                begin:
                var elemFrom = Element.ParseValues(FromDataArr[i]);
                var elemTo = new Element(ToDataArr[i]);

                if (elemFrom.Value == elemTo.DoubleValue)
                    goto end;

                DoubleAnimation value = prototype.Clone();
                value.From = elemFrom.Value;
                value.To = elemTo.DoubleValue;
                storyboard.Children.Add(value);
                Storyboard.SetTargetProperty(value, new PropertyPath(Element.ValueProperty));
                Storyboard.SetTarget(value, elemTo);

                elemTo.i = i;
                elemTo.ValueChanged += (n, d, s) => { SetDataValue(n, s); };
                end:;
            }

            return storyboard;
        }

        public class Element : UIElement
        {
            public static char[] ValueChars = "0123456789.-+".ToCharArray();
            public struct Values
            {
                public double Value;
                public string Mask;
            }

            public Element(string value)
            {
                var values = ParseValues(value);
                DoubleValue = values.Value;
                _ValueMask = values.Mask;
            }

            public static Values ParseValues(string value)
            {
                bool IsWriting = false;
                int ValueStart = 0;
                Values values = new Values();
                for (var i = 0; i < value.Length; i++)
                {
                    if (Array.IndexOf(ValueChars, value[i]) != -1 && !IsWriting)
                    {
                        IsWriting = true;
                        ValueStart = i;
                    }
                    if (IsWriting && Array.IndexOf(ValueChars, value[i]) == -1)
                    {
                        values.Value = Convert.ToDouble(value.Substring(ValueStart, i - ValueStart - 1).Replace(".", ","));
                        values.Mask = value.Remove(ValueStart, i - ValueStart + 1).Insert(ValueStart, "{0}");
                        return values;
                    }
                    if (i == value.Length - 1)
                    {
                        values.Value = Convert.ToDouble(value.Substring(ValueStart).Replace(".", ","));
                        values.Mask = value.Remove(ValueStart).Insert(ValueStart, "{0}");
                        return values;
                    }
                }
                return values;
            }

            public int i = 0;
            public string Value
            {
                get
                {
                    return string.Format(_ValueMask, DoubleValue).Replace(",", ".");
                }
            }
            public double DoubleValue;
            public string _ValueMask;

            public delegate void ValueChangedHandler(int i, double newDoubleValue, string newValue);
            public event ValueChangedHandler ValueChanged;

            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("ValueProperty", typeof(double), typeof(Element),
                new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));
            public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                Element self = (Element)d;
                self.DoubleValue = (double)e.NewValue;
                self.ValueChanged?.Invoke(self.i, self.DoubleValue, self.Value);
            }
        }

        /*public static Storyboard CreateStoryboard(PathGeometry Original, PathFigureCollection From, PathFigureCollection To)
        {
            Original.Figures = From;

            var maxIndex = From.Count >= To.Count ? From.Count : To.Count;

            Storyboard storyboard = new Storyboard();

            for (var i = 0; i < maxIndex; i++)
            {
                var maxSegmentIndex = From[i].Segments.Count >= To[i].Segments.Count ? From[i].Segments.Count : To[i].Segments.Count;
                for (var ii = 0; ii < maxSegmentIndex; ii++)
                {
                    if (Original.Figures[i].Segments[ii].GetType() == typeof(PolyLineSegment))
                    {
                        var elem = Original.Figures[i].Segments[ii] as PolyLineSegment;
                        var to = To[i].Segments[ii] as PolyLineSegment;

                        for (var iii = 0; iii < to.Points.Count; iii++)
                        {
                            PointAnimation point = new PointAnimation();
                            point.Duration = TimeSpan.FromMilliseconds(200);
                            point.From = elem.Points[iii];
                            point.To = to.Points[iii];
                            storyboard.Children.Add(point);

                            Storyboard.SetTargetProperty(point, new PropertyPath());
                            Storyboard.SetTarget(point, elem);
                        }
                    }
                }
            }

            return storyboard;
        }*/
    }
}
