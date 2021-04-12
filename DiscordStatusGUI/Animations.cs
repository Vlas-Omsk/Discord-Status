using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI
{
    class Animations
    {
        public static readonly Color DiscordLPWrongOrEmpty = Color.FromRgb(240, 71, 71);
        public static readonly Color DiscordLPLabelDefault = Color.FromRgb(138, 142, 147);
        public static readonly Color DiscordLPTextBoxDefault = Color.FromRgb(171, 173, 179);

        public static Storyboard BrushColorTo(Color to, DependencyProperty property, FrameworkElement control, int ms = 100)
        {
            Storyboard storyboard = new Storyboard();
            
            BrushAnimation brushAnimation = new BrushAnimation() { To = new SolidColorBrush(to), Duration = TimeSpan.FromMilliseconds(ms) };
            storyboard.Children.Add(brushAnimation);

            Storyboard.SetTargetProperty(brushAnimation, new PropertyPath(property));
            Storyboard.SetTarget(brushAnimation, control);

            return storyboard;
        }

        public static Storyboard VisibleOn(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 0;
            var scale = new ScaleTransform(0.7, 0.7);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    scale,
                    new SkewTransform(),
                    new RotateTransform()
                }
            };

            Storyboard storyboard = new Storyboard();

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 0;
            opacity.To = 1;
            storyboard.Children.Add(opacity);

            DoubleAnimation scalex = new DoubleAnimation();
            scalex.Duration = TimeSpan.FromMilliseconds(120);
            scalex.From = 0.7;
            scalex.To = 1;
            scalex.EasingFunction = new BackEase();
            storyboard.Children.Add(scalex);

            DoubleAnimation scaley = new DoubleAnimation();
            scaley.Duration = TimeSpan.FromMilliseconds(120);
            scaley.From = 0.7;
            scaley.To = 1;
            scaley.EasingFunction = new BackEase();
            storyboard.Children.Add(scaley);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaley, control);

            Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scalex, control);

            return storyboard;
        }

        public static Storyboard VisibleOff(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 1;
            var scale = new ScaleTransform(1, 1);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    scale,
                    new SkewTransform(),
                    new RotateTransform()
                }
            };

            Storyboard storyboard = new Storyboard();

            DoubleAnimation scalex = new DoubleAnimation();
            scalex.Duration = TimeSpan.FromMilliseconds(120);
            scalex.From = 1;
            scalex.To = 1.3;
            storyboard.Children.Add(scalex);

            DoubleAnimation scaley = new DoubleAnimation();
            scaley.Duration = TimeSpan.FromMilliseconds(120);
            scaley.From = 1;
            scaley.To = 1.3;
            storyboard.Children.Add(scaley);

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 1;
            opacity.To = 0;
            opacity.Completed += (s, e) => { control.Visibility = Visibility.Hidden; };
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaley, control);

            Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scalex, control);

            return storyboard;
        }

        public static Storyboard VisibleOnZoomSlideDown(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 0;
            var translate = new TranslateTransform();
            var scale = new ScaleTransform(0.7, 0.7);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    scale,
                    new SkewTransform(),
                    new RotateTransform(),
                    translate
                }
            };
            translate.Y = -100;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation pathY = new DoubleAnimation();
            pathY.Duration = TimeSpan.FromMilliseconds(120);
            pathY.From = -100;
            pathY.To = 0;
            storyboard.Children.Add(pathY);

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 0;
            opacity.To = 1;
            storyboard.Children.Add(opacity);

            DoubleAnimation scalex = new DoubleAnimation();
            scalex.Duration = TimeSpan.FromMilliseconds(120);
            scalex.From = 0.7;
            scalex.To = 1;
            storyboard.Children.Add(scalex);

            DoubleAnimation scaley = new DoubleAnimation();
            scaley.Duration = TimeSpan.FromMilliseconds(120);
            scaley.From = 0.7;
            scaley.To = 1;
            storyboard.Children.Add(scaley);

            Storyboard.SetTargetProperty(pathY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));
            Storyboard.SetTarget(pathY, control);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaley, control);

            Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scalex, control);

            return storyboard;
        }

        public static Storyboard VisibleOffZoomSlideDown(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 1;
            var translate = new TranslateTransform();
            var scale = new ScaleTransform(1, 1);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    scale,
                    new SkewTransform(),
                    new RotateTransform(),
                    translate
                }
            };
            translate.Y = 0;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation pathY = new DoubleAnimation();
            pathY.Duration = TimeSpan.FromMilliseconds(120);
            pathY.From = 0;
            pathY.To = 100;
            storyboard.Children.Add(pathY);

            DoubleAnimation scalex = new DoubleAnimation();
            scalex.Duration = TimeSpan.FromMilliseconds(120);
            scalex.From = 1;
            scalex.To = 0.7;
            storyboard.Children.Add(scalex);

            DoubleAnimation scaley = new DoubleAnimation();
            scaley.Duration = TimeSpan.FromMilliseconds(120);
            scaley.From = 1;
            scaley.To = 0.7;
            storyboard.Children.Add(scaley);

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 1;
            opacity.To = 0;
            opacity.Completed += (s, e) => { control.Visibility = Visibility.Hidden; };
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(pathY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));
            Storyboard.SetTarget(pathY, control);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaley, control);

            Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scalex, control);

            return storyboard;
        }

        public static Storyboard VisibleOnSlideDown(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 0;
            var translate = new TranslateTransform();
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new SkewTransform(),
                    new RotateTransform(),
                    translate
                }
            };
            translate.Y = -100;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation pathY = new DoubleAnimation();
            pathY.Duration = TimeSpan.FromMilliseconds(120);
            pathY.From = -100;
            pathY.To = 0;
            storyboard.Children.Add(pathY);

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 0;
            opacity.To = 1;
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(pathY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));
            Storyboard.SetTarget(pathY, control);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            return storyboard;
        }

        public static Storyboard VisibleOffSlideDown(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;
            control.Opacity = 1;
            var translate = new TranslateTransform();
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new SkewTransform(),
                    new RotateTransform(),
                    translate
                }
            };
            translate.Y = 0;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation pathY = new DoubleAnimation();
            pathY.Duration = TimeSpan.FromMilliseconds(120);
            pathY.From = 0;
            pathY.To = 100;
            storyboard.Children.Add(pathY);

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.From = 1;
            opacity.To = 0;
            opacity.Completed += (s, e) => { control.Visibility = Visibility.Hidden; };
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(pathY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));
            Storyboard.SetTarget(pathY, control);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, control);

            return storyboard;
        }

        public static Storyboard VisibleOnZoom(FrameworkElement control)
        {
            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(Animations.VisibleOnOpacity(control));

            storyboard.Children.Add(Animations.ScaleTransform(0.7, 1, 0.7, 1, control));

            return storyboard;
        }

        public static Storyboard VisibleOffZoom(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(Animations.VisibleOffOpacity(control));

            storyboard.Children.Add(Animations.ScaleTransform(1, 0.7, 1, 0.7, control));

            return storyboard;
        }

        public static Storyboard VisibleOnOpacity(FrameworkElement control)
        {
            control.Visibility = Visibility.Visible;

            Storyboard storyboard = new Storyboard();

            var opacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(opacity, control);

            return storyboard;
        }

        public static Storyboard VisibleOffOpacity(FrameworkElement control)
        {
            Storyboard storyboard = new Storyboard();

            var opacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
            opacity.Completed += (s, a) => 
                control.Visibility = Visibility.Hidden;
            storyboard.Children.Add(opacity);

            Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(opacity, control);

            return storyboard;
        }

        public static Storyboard Shake(int strength, int speed, TimeSpan time, FrameworkElement control)
        {
            var translate = new TranslateTransform();
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new SkewTransform(),
                    new RotateTransform(),
                    translate
                }
            };
            translate.Y = 0;
            translate.X = 0;

            Storyboard storyboard = new Storyboard();

            storyboard.RepeatBehavior = new RepeatBehavior(time);
            storyboard.SpeedRatio = speed;
            storyboard.FillBehavior = FillBehavior.Stop;

            DoubleAnimationUsingKeyFrames animX = new DoubleAnimationUsingKeyFrames();

            animX.KeyFrames.Add(new SplineDoubleKeyFrame(-strength, new TimeSpan(1000000)));
            animX.KeyFrames.Add(new SplineDoubleKeyFrame(0, new TimeSpan(3000000)));
            animX.KeyFrames.Add(new SplineDoubleKeyFrame(strength, new TimeSpan(5000000)));
            animX.KeyFrames.Add(new SplineDoubleKeyFrame(0, new TimeSpan(7000000)));

            storyboard.Children.Add(animX);

            DoubleAnimationUsingKeyFrames animY = new DoubleAnimationUsingKeyFrames();

            animY.KeyFrames.Add(new SplineDoubleKeyFrame(-strength, new TimeSpan(1000000)));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(-strength, new TimeSpan(3000000)));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(strength, new TimeSpan(5000000)));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(strength, new TimeSpan(7000000)));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(0, new TimeSpan(9000000)));

            storyboard.Children.Add(animY);

            Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)"));
            Storyboard.SetTarget(animX, control);

            Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)"));
            Storyboard.SetTarget(animY, control);

            return storyboard;
        }

        public static Storyboard ScaleTransform(double fromX, double toX, double fromY, double toY, FrameworkElement control)
        {
            ScaleTransform scale = new ScaleTransform(fromX, fromY);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.RenderTransform = scale;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation scalex = new DoubleAnimation();
            scalex.Duration = TimeSpan.FromMilliseconds(120);
            scalex.From = fromX;
            scalex.To = toX;
            storyboard.Children.Add(scalex);

            DoubleAnimation scaley = new DoubleAnimation();
            scaley.Duration = TimeSpan.FromMilliseconds(120);
            scaley.From = fromY;
            scaley.To = toY;
            storyboard.Children.Add(scaley);

            Storyboard.SetTargetProperty(scaley, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(scaley, control);

            Storyboard.SetTargetProperty(scalex, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(scalex, control);

            return storyboard;
        }

        public static void ReplaceWithWaves(Grid parent, FrameworkElement oldControl, FrameworkElement newControl, bool withremoveoldcontrol = false)
        {
            newControl.Visibility = Visibility.Hidden;

            var plug_img = new Image() { Width = parent.ActualWidth * 1.5, Height = parent.ActualHeight * 1.5,
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
                Source = BitmapEx.ToImageSource(Properties.Resources.Plug), IsHitTestVisible = true };
            var plug = new Grid() { Width = parent.ActualWidth * 3, Height = parent.ActualHeight * 3,
                Margin = new Thickness(-(parent.ActualWidth * 1.5), -(parent.ActualHeight * 1.5), 0, 0),
                Background = new SolidColorBrush(Colors.Transparent), IsHitTestVisible = true };
            plug.Children.Add(plug_img);

            parent.Children.Add(plug);

            var opacity = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            var location = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(1000)));

            location.Completed += (s, ev) =>
            {
                if (oldControl != null)
                    oldControl.Visibility = Visibility.Hidden;
                newControl.Visibility = Visibility.Visible;
                plug.BeginAnimation(FrameworkElement.OpacityProperty, opacity);
            };

            opacity.Completed += (s, ev) =>
            {
                plug_img.RaiseEvent(new RoutedEventArgs(Image.UnloadedEvent));
                plug.RaiseEvent(new RoutedEventArgs(Grid.UnloadedEvent));
                parent.Children.Remove(plug);
                if (withremoveoldcontrol && oldControl != null)
                    parent.Children.Remove(oldControl);
            };

            plug.BeginAnimation(Image.MarginProperty, location);
        }
    }

    public class BrushAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType
        {
            get
            {
                return typeof(Brush);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue,
                                               object defaultDestinationValue,
                                               AnimationClock animationClock)
        {
            return GetCurrentValue(defaultOriginValue as Brush,
                                   defaultDestinationValue as Brush,
                                   animationClock);
        }
        public object GetCurrentValue(Brush defaultOriginValue,
                                      Brush defaultDestinationValue,
                                      AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
                return Brushes.Transparent;

            //use the standard values if From and To are not set 
            //(it is the value of the given property)
            defaultOriginValue = this.From ?? defaultOriginValue;
            defaultDestinationValue = this.To ?? defaultDestinationValue;

            if (animationClock.CurrentProgress.Value == 0)
                return defaultOriginValue;
            if (animationClock.CurrentProgress.Value == 1)
                return defaultDestinationValue;

            return new VisualBrush(new Border()
            {
                Width = 1,
                Height = 1,
                Background = defaultOriginValue,
                Child = new Border()
                {
                    Background = defaultDestinationValue,
                    Opacity = animationClock.CurrentProgress.Value,
                }
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }

        //we must define From and To, AnimationTimeline does not have this properties
        public Brush From
        {
            get { return (Brush)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public Brush To
        {
            get { return (Brush)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));
    }
}
