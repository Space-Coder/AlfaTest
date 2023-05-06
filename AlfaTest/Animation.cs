using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AlfaTest
{
    public class Animation
    {
        private static bool isHide = false;
        private static FrameworkElement elemntForHide;
        private static DoubleAnimation doubleAnimation;
        private static readonly CircleEase ease = new CircleEase()
        {
            EasingMode = EasingMode.EaseInOut
        };
        
        public static void ScaleAnimation(FrameworkElement element, double from, double to, double duration)
        {
            doubleAnimation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                EasingFunction = ease,
            };
            ScaleTransform scaleTransform = new ScaleTransform();
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
            element.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);
        }

        public static void OpacityAnimation(FrameworkElement element, double from, double to, double duration)
        {
            doubleAnimation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration))
            };
            isHide = to == 0 ? true : false;
            elemntForHide = element;
            doubleAnimation.Completed += OpacityAnimation_Completed;
            element.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private static void OpacityAnimation_Completed(object? sender, EventArgs e)
        {
            if (isHide)
            {
                elemntForHide.Visibility = Visibility.Collapsed;
            }
            else
            {
                elemntForHide.Visibility = Visibility.Visible;
            }
        }
    }
}
