using System.Linq;

namespace cpGames.core.RapidIoC.impl
{
    internal class Context : IContext
    {
        #region Fields
        private readonly IViewCollection _views = new ViewCollection();
        private readonly IBindingCollection _bindings = new BindingCollection();
        #endregion

        #region Constructors
        public Context(string name)
        {
            Name = name;
        }
        #endregion

        #region IContext Members
        public bool IsRoot => Name == ContextCollection.ROOT_CONTEXT_NAME;
        public string Name { get; }
        public Signal DestroyedSignal { get; } = new Signal();
        public int ViewCount => _views.ViewCount;
        public int BindingCount => _bindings.BindingCount;

        public bool RegisterView(IView view, out string errorMessage)
        {
            if (!_views.RegisterView(view, out errorMessage))
            {
                return false;
            }

            foreach (var property in view.GetInjectedProperties())
            {
                if (!property.GetInjectionKey(out var key, out errorMessage) ||
                    !Bind(key, out var binding, out errorMessage) ||
                    !binding.Subscribe(view, property, out errorMessage))
                {
                    return false;
                }
            }

            return true;
        }

        public bool UnregisterView(IView view, out string errorMessage)
        {
            if (!_views.UnregisterView(view, out errorMessage))
            {
                return false;
            }

            foreach (var property in view.GetInjectedProperties())
            {
                if (!property.GetInjectionKey(out var key, out errorMessage) ||
                    !FindBinding(key, true, out var binding, out errorMessage) ||
                    !binding.Unsubscribe(view, out errorMessage))
                {
                    return false;
                }
            }
            DestroyIfEmpty();
            return true;
        }

        public bool ClearViews(out string errorMessage)
        {
            if (!_views.ClearViews(out errorMessage))
            {
                return false;
            }
            DestroyIfEmpty();
            return true;
        }

        public bool FindBinding(IKey key, bool includeDiscarded, out IBinding binding, out string errorMessage)
        {
            return
                !IsRoot && Rapid.Contexts.Root.FindBinding(key, includeDiscarded, out binding, out errorMessage) ||
                _bindings.FindBinding(key, includeDiscarded, out binding, out errorMessage);
        }

        public bool BindingExists(IKey key)
        {
            return
                !IsRoot && Rapid.Contexts.Root.BindingExists(key) ||
                _bindings.BindingExists(key);
        }

        public bool Bind(IKey key, out IBinding binding, out string errorMessage)
        {
            if (IsRoot)
            {
                foreach (var context in Rapid.Contexts.Contexts
                    .Where(x => x.LocalBindingExists(key)))
                {
                    if (!context.MoveBindingFrom(key, this, out errorMessage))
                    {
                        binding = null;
                        return false;
                    }
                }
            }
            return
                !IsRoot && Rapid.Contexts.Root.FindBinding(key, false, out binding, out errorMessage) ||
                _bindings.Bind(key, out binding, out errorMessage);
        }

        public bool BindValue(IKey key, object value, out string errorMessage)
        {
            if (IsRoot)
            {
                foreach (var context in Rapid.Contexts.Contexts
                    .Where(x => x.LocalBindingExists(key)))
                {
                    if (!context.MoveBindingFrom(key, this, out errorMessage))
                    {
                        return false;
                    }
                }
            }
            return _bindings.BindValue(key, value, out errorMessage);
        }

        public bool MoveBindingFrom(IKey key, IBindingCollection collection, out string errorMessage)
        {
            return _bindings.MoveBindingFrom(key, collection, out errorMessage);
        }

        public bool MoveBindingTo(IBinding binding, out string errorMessage)
        {
            return _bindings.MoveBindingTo(binding, out errorMessage);
        }

        public bool Unbind(IKey key, out string errorMessage)
        {
            if (!IsRoot && Rapid.Contexts.Root.Unbind(key, out errorMessage) ||
                _bindings.Unbind(key, out errorMessage))
            {
                DestroyIfEmpty();
                return true;
            }
            return false;
        }

        public bool ClearBindings(out string errorMessage)
        {
            if (!_bindings.ClearBindings(out errorMessage))
            {
                return false;
            }
            DestroyIfEmpty();
            return true;
        }

        public bool LocalBindingExists(IKey key)
        {
            return _bindings.BindingExists(key);
        }

        public bool DestroyContext(out string errorMessage)
        {
            return
                ClearBindings(out errorMessage) &&
                ClearViews(out errorMessage);
        }
        #endregion

        #region Methods
        private void DestroyIfEmpty()
        {
            if (ViewCount == 0 && BindingCount == 0 && !IsRoot)
            {
                DestroyedSignal.Dispatch();
            }
        }

        public override string ToString()
        {
            return string.Format("Context <{0}>", Name);
        }
        #endregion
    }
}