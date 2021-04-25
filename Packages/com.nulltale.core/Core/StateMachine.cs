using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.EventSystem;
using UnityEngine;


namespace CoreLib
{
    [Serializable]
    public class StateMachine<TLabel>
    {
        private IState                     m_CurrentState;
        private Dictionary<TLabel, IState> m_StateDictionary;

        private IState m_DefaultState;
        private StateChangeDelagate m_OnStateChange;
        public delegate void StateChangeDelagate(IState current, IState next);

        public IState DefaultState
        {
            get => m_DefaultState;
            set
            {
                if (value == null || Equals(m_DefaultState, value))
                    return;

                // if current state is default state, set current state to new default state value
                if (m_CurrentState.Equals(m_DefaultState))
                    m_CurrentState      = value;

                m_DefaultState = value;
            }
        }

        public ICollection<IState> States => m_StateDictionary.Values;
        public ICollection<TLabel> Labels => m_StateDictionary.Keys;

        public StateChangeDelagate OnStateChange
        {
            get => m_OnStateChange;
            set => m_OnStateChange = value;
        }

        //////////////////////////////////////////////////////////////////////////
        public interface IState
        {
            TLabel Label { get; }
            StateMachine<TLabel> StateMachine { set; }

            void OnStart();
            void OnStop();
            void OnReEnter();
        }

        public class State : IState
        {
            public TLabel Label { get; }
            public StateMachine<TLabel> StateMachine { get; set; }

            private readonly Action m_OnStart;
            private readonly Action m_OnStop;
            private readonly Action m_OnReEnter;

            /////////////////////////////////////
            public State(TLabel label, Action onStart, Action onStop, Action onReEnter)
            {
                Label = label;
                m_OnStart = onStart;
                m_OnStop = onStop;
                m_OnReEnter = onReEnter ?? _OnReEnterDefault;
            }

            public void OnStart()
            {
                m_OnStart?.Invoke();
            }

            public void OnStop()
            {
                m_OnStop?.Invoke();
            }

            public void OnReEnter()
            {
                m_OnReEnter?.Invoke();
            }

            private void _OnReEnterDefault()
            {
                OnStop();
                OnStart();
            }
        }

        public abstract class StateObject : IState
        {
            public abstract TLabel Label { get; }
            public StateMachine<TLabel> StateMachine { get; set; }
            public bool IsActiveState => StateMachine.GetCurrentState().Equals(this);

            public virtual void OnStart() { }
            public virtual void OnStop() { }
            public virtual void OnReEnter()
            {
                OnStop();
                OnStart();
            }
        }
        
        public abstract class StateObjectMonoBehaviour : MonoBehaviour, IState
        {
            public abstract TLabel Label { get; }
            public StateMachine<TLabel> StateMachine { get; set; }
            public bool IsActiveState => StateMachine.GetCurrentState().Equals(this);

            public virtual void OnStart() { }
            public virtual void OnStop() { }
            public virtual void OnReEnter()
            {
                OnStop();
                OnStart();
            }
        }

        public abstract class StateObjectReactive<TEvent> : StateObject, IEventListener<TEvent>
        {
            public List<Reaction> Reactions
            {
                get => m_Reactions;
                set => m_Reactions = value;
            }

            private List<Reaction> m_Reactions = new List<Reaction>();

            //////////////////////////////////////////////////////////////////////////
            [Serializable]
            public abstract class Reaction : IEventListener<TEvent>
            {
                public abstract void React(TEvent e);
            }

            [Serializable]
            public class Transition : Reaction
            {
                public StateObjectReactive<TEvent> Owner { get; private set; }
                public TLabel                      DestinationState;
                public Func<TEvent, bool>  Condition;

                //////////////////////////////////////////////////////////////////////////
                public override void React(TEvent e)
                {
                    if (Condition(e))
                        Owner.StateMachine.SetState(DestinationState);
                }

                public Transition(TLabel destinationState, Func<TEvent, bool> condition, StateObjectReactive<TEvent> owner)
                {
                    DestinationState = destinationState;
                    Condition        = condition;
                    Owner            = owner;
                }
            }

            //////////////////////////////////////////////////////////////////////////
            public virtual void React(TEvent e)
            {
                foreach (var reaction in Reactions)
                {
                    reaction.React(e);

                    // stop iteration if transition triggered
                    if (IsActiveState == false)
                        break;
                }
            }

            public StateObjectReactive<TEvent> AddReaction(Reaction reaction)
            {
                Reactions.Add(reaction);
                return this;
            }

            public StateObjectReactive<TEvent> AddTransition(TLabel destinationState, TEvent trigger)
            {
                AddTransition(destinationState, e => Equals(e, trigger));
                return this;
            }

            public StateObjectReactive<TEvent> AddTransition(TLabel destinationState, Func<TEvent, bool> condition)
            {
                Reactions.Add(new Transition(destinationState, condition, this));
                return this;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public StateMachine()
        {
            // allocate
            m_StateDictionary = new Dictionary<TLabel, IState>();
            m_DefaultState    = new State(default, null, null, null);

            // set default state
            m_CurrentState = m_DefaultState;
        }

        public StateMachine(params IState[] states) : this()
        {
            foreach (var state in states)
                AddState(state);
        }

        /// <summary>
        /// Adds a state, and the delegates that should run 
        /// when the state starts, stops, 
        /// and when the state machine is updated.
        /// 
        /// Any delegate can be null, and wont be executed.
        /// </summary>
        /// <param name="label">The name of the state to add.</param>
        /// <param name="onStart">The action performed when the state is entered.</param>
        /// <param name="onStop">The action performed when the state machine is left.</param>
        /// <param name="onReEnter">The action performed when the state machine calls self.</param>
        public StateMachine<TLabel> AddState(TLabel label, Action onStart = null, Action onStop = null, Action onReEnter = null)
        {
            return AddState(new State(label, onStart, onStop, onReEnter));
        }

        public StateMachine<TLabel> AddState<T>(T state) where T : IState
        {
            state.StateMachine = this;
            m_StateDictionary[state.Label] = state;

            return this;
        }

        public StateMachine<TLabel> AddStates<T>(params T[] states) where T : IState
        {
            foreach (var state in states)
                AddState(state);

            return this;
        }

        public TStateType GetState<TStateType>() where TStateType : IState
        {
            return (TStateType)m_StateDictionary.Values.FirstOrDefault(n => n is TStateType);
        }

        public IState GetState(TLabel label)
        {
            if (m_StateDictionary.TryGetValue(label, out var state))
                return state;

            return default;
        }

        public IState GetCurrentState()
        {
            return m_CurrentState;
        }


        public T GetCurrentState<T>() where T : class
        {
            if (m_CurrentState is T state)
                return state;

            return default;
        }

        public bool TryGetState<T>(out T state) where T : IState
        {
            if (m_CurrentState is T s)
            {
                state = s;
                return true;
            }

            state = default;
            return false;
        }

        public void SetState<TStateType>() where TStateType : IState
        {
            var state = GetState<TStateType>();
            if (state != null)
                _SetState(state);
        }

        public void SetState<T>(T equalState)
        {
            var state = m_StateDictionary.Values.FirstOrDefault(n => n.Equals(equalState));
            if (state != null)
                _SetState(state);
        }

        public void SetState(TLabel key)
        {
            if (m_StateDictionary.TryGetValue(key, out var state))
                _SetState(state);
        }

        public void SetState(IState state)
        {
            if (m_StateDictionary.ContainsValue(state))
                _SetState(state);
        }

        /// <summary>
        /// Set current state to default
        /// </summary>
        public void Stop()
        {
            _SetState(DefaultState);
        }


        /// <summary>
        /// Returns the current state name
        /// </summary>
        public override string ToString()
        {
            return m_CurrentState?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Changes the state from the existing one to the given
        /// </summary>
        private void _SetState(IState state)
        {
            if (state.Equals(m_CurrentState))
            {
                // activate ReEnter
                m_CurrentState.OnReEnter();
                return;
            }

            // set new state
            m_CurrentState.OnStop();
            m_OnStateChange?.Invoke(m_CurrentState, state);
            m_CurrentState = state;
            m_CurrentState.OnStart();
        }
    }
}