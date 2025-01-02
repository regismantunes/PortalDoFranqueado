using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PortalDoFranqueado.ViewModel
{
    public class FieldViewModelCollection
    {
        private readonly object _lock = new();
        private readonly List<IFieldViewModel> _fields;
        private IFieldViewModel? _focusedField;
        private IFieldViewModel? _internalSettedFocusField;
        private readonly List<IFieldViewModel> _internalFocusedFields;

        public int Count => _fields.Count;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FieldViewModelCollection()
        {
            _fields = [];
            _internalFocusedFields = [];
        }

        public void Add(IFieldViewModel field)
        {
            field.GotFocus += Field_GotFocus;
            if (field is INotifyPropertyChanged notify)
                notify.PropertyChanged += PropertyChanged;

            _fields.Add(field);
        }

        private void Field_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            lock (_lock)
            {
                var field = (IFieldViewModel)sender;
                if (_internalSettedFocusField == null ||
                    _internalSettedFocusField == field)
                    _focusedField = field;

                _internalFocusedFields.Remove(field);
                if(_internalFocusedFields.Count == 0) 
                    _internalSettedFocusField = null;
            }
        }

        public void Clear() => _fields.Clear();

        public void RemoveAll(Predicate<IFieldViewModel> match) => _fields.RemoveAll(match);

        public void Remove(IFieldViewModel field) => _fields.Remove(field);

        private void SetInternalFocusedField(IFieldViewModel field)
        {
            _internalFocusedFields.Add(field);
            field.IsFocused = true;
        }

        public void GoToNext()
        {
            lock (_lock)
            {
                if (_fields.Count == 0)
                    return;

                var i = -1;
                if (_focusedField != null)
                {
                    _focusedField.IsFocused = false;
                    i = _fields.IndexOf(_focusedField);
                }

                if (i == _fields.Count - 1)
                    i = -1;

                SetInternalFocusedField(_fields[i + 1]);
            }
        }

        public void GoToPrevius()
        {
            lock (_lock)
            {
                if (_fields.Count == 0)
                    return;

                var i = -1;
                if (_focusedField != null)
                {
                    _focusedField.IsFocused = false;
                    i = _fields.IndexOf(_focusedField);
                }

                if (i < 1)
                    i = _fields.Count;

                SetInternalFocusedField(_fields[i - 1]);
            }
        }

        public void GoToFirst()
        {
            lock (_lock)
            {
                if (_fields.Count == 0)
                    return;

                if (_focusedField != null)
                    _focusedField.IsFocused = false;

                SetInternalFocusedField(_fields[0]);
            }
        }

        public void GoToLast()
        {
            lock (_lock)
            {
                if (_fields.Count == 0)
                    return;

                if (_focusedField != null)
                    _focusedField.IsFocused = false;

                SetInternalFocusedField(_fields[^1]);
            }
        }

        public void SetFocus(IFieldViewModel field)
        {
            lock (_lock)
            {
                if (_fields.Count == 0)
                    return;

                var i = _fields.IndexOf(field);
                if (i < 0)
                    return;

                if (_focusedField != null)
                    _focusedField.IsFocused = false;

                _internalSettedFocusField = field;
                SetInternalFocusedField(field);
            }
        }
    }
}