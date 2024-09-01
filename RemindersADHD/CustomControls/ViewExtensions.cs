namespace RemindersADHD.CustomControls
{
    public static class ViewExtensions
    {
        public static Task<bool> NumberTo(this VisualElement self, double fromValue, double toValue, Action<double> callback, uint rate = 16, uint length = 250, Easing? easing = null, string animationName = "NumberTo")
        {
            Func<double, double> transform = t => fromValue + t * (toValue - fromValue);
            return NumberAnimation(self, animationName, transform, callback, rate, length, easing);
        }
        static Task<bool> NumberAnimation(VisualElement element, string name, Func<double, double> transform, Action<double> callback, uint rate, uint length, Easing? easing)
        {
            easing ??= Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate<double>(name, transform, callback, rate, length, easing, (v, c) => taskCompletionSource.SetResult(true));
            return taskCompletionSource.Task;
        }
        public static Task<bool> NumberTo(this VisualElement self, long fromValue, long toValue, Action<long> callback, uint rate = 16, uint length = 250, Easing? easing = null, string animationName = "NumberTo")
        {
            Func<double, long> transform = t => (long)(fromValue + t * (toValue - fromValue));
            return NumberAnimation(self, animationName, transform, callback, rate, length, easing);
        }
        static Task<bool> NumberAnimation(VisualElement element, string name, Func<double, long> transform, Action<long> callback, uint rate, uint length, Easing? easing)
        {
            easing ??= Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate<long>(name, transform, callback, rate, length, easing, (v, c) => taskCompletionSource.SetResult(true));
            return taskCompletionSource.Task;
        }
    }
}
