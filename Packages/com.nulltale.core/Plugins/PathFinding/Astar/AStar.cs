using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoreLib;

namespace AStar
{
	public interface iExplorer<T>
	{
		IEnumerable<T> GetNeighbours(T node);
		float GetPathCost(T start, T goal);
		float GetShortestPath(T start, T goal);
		bool Reachable(T start, T goal);
        bool Passable(T node);
	}

	public class PathNode<T> : FastPriorityQueueNode
    {
        // previous node
		public PathNode<T>          СameFrom;
        // estimated length to target
		public float                PathCostEstimated;
        // length from start, used Priority instead
		public float                PathCost;
        // link to owner
        public readonly T           Master;
        // priority of this node
        public float                Cost;

		//////////////////////////////////////////////////////////////////////////
		public PathNode(T master)
		{
			Master = master;
		}
    }

	public class FindPathProcess<T>
	{
		public  LinkedList<T>                  Path      { get; } = new LinkedList<T>();
		public  FastPriorityQueue<PathNode<T>> OpenSet   { get; private set; }
		public  HashSet<T>                     ClosedSet { get; private set; }
        private LinkedList<PathNode<T>>        ClosedNodes;
        public  T                              Start    { get; private set; }
        public  T[]                            Goal     { get; private set; }
        public  iExplorer<T>                   Explorer { get; private set; }
        public  FindPathOptions                Options  { get; private set; }

        public Func<T, T, bool>                GoalCheck { get; set; }

        public FindPathProcessState            State { get; private set; }

        public bool                            IsPathFound => State == FindPathProcessState.Found;

        //////////////////////////////////////////////////////////////////////////
        public class FindPathYieldInstruction : CustomYieldInstruction
        {
            private FindPathProcess<T>         m_FindPathProcess;
            private Action<FindPathProcess<T>> m_OnComplete;

            public override bool keepWaiting 
            { 
                get
                {
                    // implement pathfinding
                    if (m_FindPathProcess.State == FindPathProcessState.Running)
                    {
                        m_FindPathProcess._ImplementPathfinding((int)(Pathfinder.s_AdaptiveBufferExperiance.Average * m_FindPathProcess.Options.ChecksMeasure));
                    }
                
                    // wait if running, activate onComplete if over
                    var isRunning = m_FindPathProcess.State == FindPathProcessState.Running;
                    if (isRunning == false)
                        m_OnComplete?.Invoke(m_FindPathProcess);

                    return isRunning;
                }
            }

            public FindPathYieldInstruction SetOnComplete(Action<FindPathProcess<T>> action)
            {
                m_OnComplete = action;
                return this;
            }

            public FindPathYieldInstruction(FindPathProcess<T> findPathProcess)
            {
                m_FindPathProcess = findPathProcess;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        internal void Init(FindPathOptions options, iExplorer<T> exlorer, T start, Func<T, T, bool> goalCheck, T[] goal)
        {
            if (State == FindPathProcessState.None)
            {
                // save data
                Options = options;
                Explorer = exlorer;
                Start = start;
                Goal = goal;
                GoalCheck = goalCheck;

                // set running state
                State = FindPathProcessState.Ready;


                // allocate collections
                OpenSet = new FastPriorityQueue<PathNode<T>>(Pathfinder.s_AdaptiveBufferExperiance.Average);
                ClosedSet = new HashSet<T>();
                ClosedNodes = new LinkedList<PathNode<T>>();

                // create start node, add to open set
                var startNode = new PathNode<T>(start)
                {
                    СameFrom = null,
                    PathCost = 0.0f,
                    PathCostEstimated = 0.0f,
                    Cost = 0.0f
                };

                OpenSet.Enqueue(startNode, startNode.Cost);
            }
        }

        internal void Complete(FindPathProcessState processState)
        {
            // set state
            State = processState;

            // impact to adaptive buffer size
            if (OpenSet != null)
            {
                switch (processState)
                {
                    case FindPathProcessState.Found:
                    {
                        Pathfinder.s_AdaptiveBufferExperiance.AddExperiance((int)((OpenSet.Count + ClosedSet.Count) * 1.2f));
                    }   break;
                    case FindPathProcessState.Interrupted:
                        break;
                    case FindPathProcessState.BadArguments:
                    case FindPathProcessState.NotReachable:
                    case FindPathProcessState.Overflow:
                        break;
                    case FindPathProcessState.None:
                    case FindPathProcessState.Running:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(processState), processState, null);
                }
            }
        }

        public FindPathProcess<T> Implement()
        {
            // do while has variants
            _ImplementPathfinding(int.MaxValue);

            return this;
        }

        public FindPathYieldInstruction Yield()
        {
            if (State == FindPathProcessState.Ready)
                State = FindPathProcessState.Running;
            return new FindPathYieldInstruction(this);
        }

        public bool TryImplement(out IEnumerable<T> path)
        {
            if (IsPathFound == false)
                Implement();

            if (IsPathFound)
            {
                path = Path;
                return true;
            }
            else
            {
                path = default;
                return false;
            }
        }

        internal bool Validate(bool connectionCheck, bool passableCheck)
        {
            // path found
            if (Path == null)
                return false;

            // all node passable
            if (passableCheck)
                if (Path.Any(node => Explorer.Passable(node) == false))
                    return false;

            // connection exists
            if (connectionCheck)
                for (var node = Path.First; node.Next != null; node = node.Next)
                {
                    if (Explorer.GetNeighbours(node.Value).Contains(node.Next.Value) == false)
                        return false;
                }

            return true;
        }

        internal LinkedList<T> BuildClosestPath()
        {
            if (Path.Count > 0)
                return Path;
                
            // get closest node from closed set
            var closestNode = ClosedNodes.MinBy(node => Goal.Min(goal => Explorer.GetShortestPath(node.Master, goal)));
            
            for (var currentNode = closestNode; currentNode != null; currentNode = currentNode.СameFrom)
                Path.AddFirst(currentNode.Master);

            return Path;
        }

        //////////////////////////////////////////////////////////////////////////
        private void _ImplementPathfinding(int checks)
        {
            if (checks <= 0)
                checks = 1;

            // do while has variants
            while (checks-- > 0)
            {
                // not found
                if (OpenSet.Count == 0)
                {
                    Complete(FindPathProcessState.NotReachable);
                    return;
                }

                // limit achieved
                if (ClosedSet.Count >= Options.CheckLimit)
                {
                    Complete(FindPathProcessState.Overflow);
                    return;
                }

                // get next node
                var currentNode = OpenSet.First();

                // close current, move from open to close set
                OpenSet.Remove(currentNode);
                ClosedSet.Add(currentNode.Master);
                ClosedNodes.AddLast(currentNode);

                // goal check
                if (Goal.Any(n => GoalCheck(n, currentNode.Master)))
                {
                    getPath(currentNode, Path);
                    Complete(FindPathProcessState.Found);
                    return;
                }

                // proceed connections
                foreach (var neighborNode in Explorer.GetNeighbours(currentNode.Master))
                {
                    // skip if not passable
                    if (neighborNode == null || Explorer.Passable(neighborNode) == false)
                        continue;

                    // IsClosed, skip if already checked
                    if (ClosedSet.Contains(neighborNode))
                        continue;
					
                    var pathCost = currentNode.PathCost + Explorer.GetPathCost(currentNode.Master, neighborNode);

                    var openNode = OpenSet.FirstOrDefault(pathNode => pathNode.Master.Equals(neighborNode));
                    if (openNode != null)
                    {	
                        // if presented and part is shorter, then reset his parent and cost
                        if (openNode.PathCost > pathCost)
                        {
                            openNode.СameFrom = currentNode;
                            openNode.PathCost = pathCost;
                            // update priority
                            openNode.Cost = openNode.PathCostEstimated + openNode.PathCost;
                            OpenSet.UpdatePriority(openNode, openNode.Cost);
                        }
                    }
                    else
                    {	
                        // if not presented, add as variant
                        var pathNode = new PathNode<T>(neighborNode);
                        pathNode.СameFrom = currentNode;
                        pathNode.PathCost = pathCost;
                        if (Options.Heuristic)
                        {
                            pathNode.PathCostEstimated = getShortestPath(Explorer, pathNode.Master, Goal);
                            pathNode.Cost = pathNode.PathCostEstimated + pathNode.PathCost;
                        }
                        else
                            pathNode.Cost = pathNode.PathCost;
                        OpenSet.Enqueue(pathNode, pathNode.Cost);
                    }
                }
            }
            
            /////////////////////////////////////
            float getShortestPath(iExplorer<T> explorer, T start, IEnumerable<T> goal)
            {
                var shortestPath = float.MaxValue;

                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var n in goal)
                {
                    var currentShortestPath = explorer.GetShortestPath(start, n);
                    if (shortestPath > currentShortestPath)
                        shortestPath = currentShortestPath;
                }

                return shortestPath;
            }
            
            void getPath(PathNode<T> pathNode, LinkedList<T> path)
            {
                for (var currentNode = pathNode; currentNode != null; currentNode = currentNode.СameFrom)
                    path.AddFirst(currentNode.Master);
            }
        }

    }

    [Serializable]
    public enum FindPathProcessState
    {
        None,
        Ready,

        Running,

        Found,

        Interrupted,
        BadArguments,
        NotReachable,
        Overflow,
    }

    [Serializable]
    public class FindPathOptions
    {
        public static readonly FindPathOptions k_Default       = new FindPathOptions();
        
        public float                           ChecksMeasure   = 1.0f / 15.0f;
        public int                             CheckLimit      = 256;
        public bool                            Heuristic       = true;
    }

    //////////////////////////////////////////////////////////////////////////
	public static class Pathfinder
	{
        public static HistoryBufferInt         s_AdaptiveBufferExperiance = new HistoryBufferInt(32, 256);

        //////////////////////////////////////////////////////////////////////////
        public static FindPathProcess<T> FindPath<T>(this iExplorer<T> explorer, T start, params T[] goal)
        {
            return explorer.FindPath(FindPathOptions.k_Default, (a, b) => a.Equals(b), start, goal);
        }

        public static FindPathProcess<T> FindPathGoalCheck<T>(this iExplorer<T> explorer, Func<T, T, bool> goalCheck, T start, params T[] goal)
        {
            return explorer.FindPath(FindPathOptions.k_Default, goalCheck, start, goal);
        }

        public static FindPathProcess<T> FindPath<T>(this iExplorer<T> explorer, out FindPathProcess<T> findPathProcess, T start, params T[] goal)
        {
            findPathProcess = explorer.FindPath(start, goal);
            return findPathProcess;
        }

        public static FindPathProcess<T> FindPath<T>(this iExplorer<T> explorer, out FindPathProcess<T> findPathProcess, FindPathOptions options, T start, params T[] goal)
        {
            findPathProcess = explorer.FindPath(options, (a, b) => a.Equals(b), start, goal);
            return findPathProcess;
        }

		public static FindPathProcess<T> FindPath<T>(this iExplorer<T> explorer, FindPathOptions options, Func<T, T, bool> goalCheck, T start, params T[] goal)
		{
            // set default options
            if (options == null)
                options = FindPathOptions.k_Default;

            if (goalCheck == null)
                goalCheck = (a, b) => a.Equals(b);

            // create result
            var result = new FindPathProcess<T>();

            // common sense check
			if (start == null || goal.Length == 0 || explorer == null)
            {
                result.Complete(FindPathProcessState.BadArguments);
                return result;
            }

            // find only reachable goals
            goal = goal
                .Where(n => n != null && explorer.Reachable(start, n))
                .ToArray();

            // is reachable
            if (goal.Length == 0)
            {
                result.Complete(FindPathProcessState.NotReachable);
                return result;
            }

            // init result inner data
            result.Init(options, explorer, start, goalCheck, goal);
            
            return result;
        }
    }
}