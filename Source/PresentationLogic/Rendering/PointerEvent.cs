namespace PresentationLogic.Rendering
{
    public enum PointerButton { None, Left, Middle, Right }

    public readonly struct PointerEvent
    {
        public float X { get; }
        public float Y { get; }
        public PointerButton Button { get; }
        public int Delta { get; }

        public PointerEvent(float x, float y, PointerButton button = PointerButton.None, int delta = 0)
        {
            X = x; Y = y; Button = button; Delta = delta;
        }
    }

    public readonly struct KeyEvent
    {
        public int KeyValue { get; }
        public KeyEvent(int keyValue) { KeyValue = keyValue; }
    }
}
