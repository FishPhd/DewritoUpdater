using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DewritoUpdater
{
    /// <summary>
    ///     Base class for view models.
    /// </summary>
    /// <remarks>http://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist</remarks>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event, getting the property name from a selector expression.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="selectorExpression">The selector expression.</param>
        /// <exception cref="System.ArgumentNullException">selectorExpression</exception>
        /// <exception cref="System.ArgumentException">The body must be a member expression</exception>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException("selectorExpression");
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(body.Member.Name);
        }

        /// <summary>
        ///     Sets a field wrapped by a property, raising the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The new value.</param>
        /// <param name="selectorExpression">The selector expression to get the property name from.</param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(selectorExpression);
            return true;
        }
    }
}