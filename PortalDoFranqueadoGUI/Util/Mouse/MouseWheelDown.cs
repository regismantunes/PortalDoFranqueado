using System.Windows.Input;

namespace PortalDoFranqueado.Util.Mouse
{
    public class MouseWheelDown : MouseGesture
    {
        public MouseWheelDown()
            : base(MouseAction.WheelClick)
        { }

        public MouseWheelDown(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        { }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs))
                return false;

            return inputEventArgs is MouseWheelEventArgs args &&
                args.Delta < 0;
        }
    }
}
