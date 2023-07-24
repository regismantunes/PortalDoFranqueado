using System.Windows.Input;

namespace PortalDoFranqueado.Util.Mouse
{
    public class MouseWheelUp : MouseGesture
    {
        public MouseWheelUp() 
            : base(MouseAction.WheelClick)
        { }

        public MouseWheelUp(ModifierKeys modifiers) 
            : base(MouseAction.WheelClick, modifiers)
        { }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs))
                return false;

            return inputEventArgs is MouseWheelEventArgs args && 
                args.Delta > 0;
        }
    }
}
