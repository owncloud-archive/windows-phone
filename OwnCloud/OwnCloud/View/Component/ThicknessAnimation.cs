using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

namespace OwnCloud
{
    /// <summary>
    /// ThicknessAnimation for Windows Phone
    /// Author: Ben Lemmon
    /// Source: http://blogs.msdn.com/b/blemmon/archive/2009/03/18/animating-margins-in-silverlight.aspx
    /// </summary>
    public class ThicknessAnimation
    {
        // The time along the animation from 0-1
        public static DependencyProperty TimeProperty = DependencyProperty.RegisterAttached("Time",
          typeof(double), typeof(DoubleAnimation), new PropertyMetadata(OnTimeChanged));
        // The object being animated
        public static DependencyProperty TargetProperty = DependencyProperty.RegisterAttached("Target",
          typeof(DependencyObject), typeof(ThicknessAnimation), null);
        // The thickness we're animating to
        public static DependencyProperty FromProperty = DependencyProperty.RegisterAttached("From",
          typeof(Thickness), typeof(DependencyObject), null);
        // The tickness we're animating from
        public static DependencyProperty ToProperty = DependencyProperty.RegisterAttached("To",
          typeof(Thickness), typeof(DependencyObject), null);
        // The target property to animate to.  Should have a property type of Thickness
        public static DependencyProperty TargetPropertyProperty = DependencyProperty.RegisterAttached(
          "TargetProperty", typeof(DependencyProperty), typeof(DependencyObject), null);

        public static Timeline Create(DependencyObject target, DependencyProperty targetProperty,
          Duration duration, Thickness from, Thickness to)
        {
            DoubleAnimation timeAnimation = new DoubleAnimation() { From = 0, To = 1, Duration = duration };
            timeAnimation.EasingFunction = new ExponentialEase()
            {
                Exponent = 9,
                EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
            };
            timeAnimation.SetValue(TargetProperty, target);
            timeAnimation.SetValue(TargetPropertyProperty, targetProperty);
            timeAnimation.SetValue(FromProperty, from);
            timeAnimation.SetValue(ToProperty, to);
            Storyboard.SetTargetProperty(timeAnimation, new PropertyPath("(ThicknessAnimation.Time)"));
            Storyboard.SetTarget(timeAnimation, timeAnimation);
            return timeAnimation;
        }

        private static void OnTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DoubleAnimation animation = (DoubleAnimation)sender;
            double time = GetTime(animation);
            Thickness from = (Thickness)sender.GetValue(FromProperty);
            Thickness to = (Thickness)sender.GetValue(ToProperty);
            DependencyProperty targetProperty = (DependencyProperty)sender.GetValue(TargetPropertyProperty);
            DependencyObject target = (DependencyObject)sender.GetValue(TargetProperty);
            target.SetValue(targetProperty, new Thickness((to.Left - from.Left) * time + from.Left,
                                                          (to.Top - from.Top) * time + from.Top,
                                                          (to.Right - from.Right) * time + from.Right,
                                                          (to.Bottom - from.Bottom) * time + from.Bottom));
        }

        public static double GetTime(DoubleAnimation animation)
        {
            return (double)animation.GetValue(TimeProperty);
        }

        public static void SetTime(DoubleAnimation animation, double value)
        {
            animation.SetValue(TimeProperty, value);
        }
    }   
}
