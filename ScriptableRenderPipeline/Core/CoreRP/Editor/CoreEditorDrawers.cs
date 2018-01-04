﻿using System;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

namespace UnityEditor.Experimental.Rendering
{
    public static class CoreEditorDrawer<TUIState, TData>
    {
        public interface IDrawer
        {
            void Draw(TUIState s, TData p, Editor owner);
        }

        public delegate T2UIState StateSelect<T2UIState>(TUIState s, TData d, Editor o);
        public delegate T2Data DataSelect<T2Data>(TUIState s, TData d, Editor o);

        public delegate void ActionDrawer(TUIState s, TData p, Editor owner);
        public delegate float FloatGetter(TUIState s, TData p, Editor owner, int i);
        public delegate AnimBool AnimBoolGetter(TUIState s, TData p, Editor owner);

        public static readonly IDrawer space = Action((state, data, owner) => EditorGUILayout.Space());
        public static readonly IDrawer noop = Action((state, data, owner) => { });

        public static IDrawer Action(params ActionDrawer[] drawers)
        {
            return new ActionDrawerInternal(drawers);
        }

        public static IDrawer FadeGroup(FloatGetter fadeGetter, bool indent, params IDrawer[] groupDrawers)
        {
            return new FadeGroupsDrawerInternal(fadeGetter, indent, groupDrawers);
        }

        public static IDrawer FoldoutGroup(string title, AnimBoolGetter root, bool indent, params IDrawer[] bodies)
        {
            return new FoldoutDrawerInternal(title, root, indent, bodies);
        }

        public static IEnumerable<IDrawer> Select<T2UIState, T2Data>(
            StateSelect<T2UIState> stateSelect,
            DataSelect<T2Data> dataSelect,
            params CoreEditorDrawer<T2UIState, T2Data>.IDrawer[] otherDrawers)
        {
            var result = new IDrawer[otherDrawers.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = new SelectDrawerInternal<T2UIState, T2Data>(stateSelect, dataSelect, otherDrawers[i]);
            return result;
        }

        public static IDrawer SelectSingle<T2UIState, T2Data>(
            StateSelect<T2UIState> stateSelect,
            DataSelect<T2Data> dataSelect,
            CoreEditorDrawer<T2UIState, T2Data>.IDrawer otherDrawers)
        {
            return new SelectDrawerInternal<T2UIState, T2Data>(stateSelect, dataSelect, otherDrawers);
        }

        class SelectDrawerInternal<T2UIState, T2Data> : IDrawer
        {
            StateSelect<T2UIState> m_StateSelect;
            DataSelect<T2Data> m_DataSelect;
            CoreEditorDrawer<T2UIState, T2Data>.IDrawer[] m_SourceDrawers;

            public SelectDrawerInternal(StateSelect<T2UIState> stateSelect,
                DataSelect<T2Data> dataSelect,
                params CoreEditorDrawer<T2UIState, T2Data>.IDrawer[] otherDrawers)
            {
                m_SourceDrawers = otherDrawers;
                m_StateSelect = stateSelect;
                m_DataSelect = dataSelect;
            }

            void IDrawer.Draw(TUIState s, TData p, Editor o)
            {
                for (var i = 0; i < m_SourceDrawers.Length; i++)
                    m_SourceDrawers[i].Draw(m_StateSelect(s, p, o), m_DataSelect(s, p, o), o);
            }
        }

        class ActionDrawerInternal : IDrawer
        {
            ActionDrawer[] actionDrawers { get; set; }
            public ActionDrawerInternal(params ActionDrawer[] actionDrawers)
            {
                this.actionDrawers = actionDrawers;
            }

            void IDrawer.Draw(TUIState s, TData p, Editor owner)
            {
                for (var i = 0; i < actionDrawers.Length; i++)
                    actionDrawers[i](s, p, owner);
            }
        }

        class FadeGroupsDrawerInternal : IDrawer
        {
            IDrawer[] groupDrawers;
            FloatGetter getter;
            bool indent;

            public FadeGroupsDrawerInternal(FloatGetter getter, bool indent, params IDrawer[] groupDrawers)
            {
                this.groupDrawers = groupDrawers;
                this.getter = getter;
                this.indent = indent;
            }

            void IDrawer.Draw(TUIState s, TData p, Editor owner)
            {
                for (var i = 0; i < groupDrawers.Length; ++i)
                {
                    if (EditorGUILayout.BeginFadeGroup(getter(s, p, owner, i)))
                    {
                        if (indent)
                            ++EditorGUI.indentLevel;
                        groupDrawers[i].Draw(s, p, owner);
                        if (indent)
                            --EditorGUI.indentLevel;
                    }
                    EditorGUILayout.EndFadeGroup();
                }
            }
        }

        class FoldoutDrawerInternal : IDrawer
        {
            IDrawer[] bodies;
            AnimBoolGetter isExpanded;
            string title;
            bool indent;

            public FoldoutDrawerInternal(string title, AnimBoolGetter isExpanded, bool indent, params IDrawer[] bodies)
            {
                this.title = title;
                this.isExpanded = isExpanded;
                this.bodies = bodies;
                this.indent = indent;
            }

            public void Draw(TUIState s, TData p, Editor owner)
            {
                var r = isExpanded(s, p, owner);
                CoreEditorUtils.DrawSplitter();
                r.target = CoreEditorUtils.DrawHeaderFoldout(title, r.target);
                if (EditorGUILayout.BeginFadeGroup(r.faded))
                {
                    if (indent)
                        ++EditorGUI.indentLevel;
                    for (var i = 0; i < bodies.Length; i++)
                        bodies[i].Draw(s, p, owner);
                    if (indent)
                        --EditorGUI.indentLevel;
                }
                EditorGUILayout.EndFadeGroup();
            }
        }
    }

    public static class CoreEditorDrawersExtensions
    {
        public static void Draw<TUIState, TData>(this IEnumerable<CoreEditorDrawer<TUIState, TData>.IDrawer> drawers, TUIState s, TData p, Editor o)
        {
            foreach (var drawer in drawers)
                drawer.Draw(s, p, o);
        }
    }
}
