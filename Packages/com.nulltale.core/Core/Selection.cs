using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreLib
{
    public class Selection
    {
        private HashSet<ISelectable>             m_Selected = new HashSet<ISelectable>();
        public  IReadOnlyCollection<ISelectable> Selected => m_Selected;

        //////////////////////////////////////////////////////////////////////////
        public void Set(params ISelectable[] selected)
        {
            // remove not presented
            m_Selected.RemoveWhere(n =>
            {
                if (selected.Contains(n) == false)
                {
                    n.Deselect();
                    return true;
                }

                return false;
            });

            // select other
            Select(selected);

        }

        public void Select(params ISelectable[] toSelect)
        {
            foreach (var item in toSelect)
            {
                if (m_Selected.Contains(item) == false)
                {
                    if (item != null)
                    {
                        m_Selected.Add(item);
                        item.Select();
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var selectable in m_Selected)
                selectable.Deselect();
            m_Selected.Clear();
        }
    }

    public interface ISelectable
    {
        void Select(); 
        void Deselect();
    }
}