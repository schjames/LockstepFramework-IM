﻿//=======================================================================
// Copyright (c) 2015 John Pan
// Distributed under the MIT License.
// (See accompanying file LICENSE or copy at
// http://opensource.org/licenses/MIT)
//=======================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Lockstep
{
    public class GridNode
    {
        #region Constructor

        static GridNode()
        {
            /*for (i = -1; i <= 1; i++) {
				for (j = -1; j <= 1; j++) {
					
					if (i == 0 && j == 0)
						continue;
					if ((i != 0 && j != 0))
						continue;
				}
			}*/
        }

        public GridNode(int _x, int _y)
        {
            gridX = _x;
            gridY = _y;
            gridIndex = gridX * GridManager.NodeCount + gridY;
            WorldPos.x = gridX * FixedMath.One + GridManager.OffsetX;
            WorldPos.y = gridY * FixedMath.One + GridManager.OffsetY;
        }
        public void Setup () {
            GenerateNeighbors();
            LinkedScanNode = GridManager.GetScanNode(gridX / GridManager.ScanResolution, gridY / GridManager.ScanResolution);
        }
        public void Initialize()
        {

        }

        #endregion


        #region Collection Helpers

        public uint ClosedSetVersion;
        public uint HeapVersion;
        public uint HeapIndex;

        #endregion


        #region Pathfinding

        const int taxPerUnit = 25;

        public int gridX;
        public int gridY;
        public int gridIndex;

        public int gCost
        {
            get
            {
                return _gCost + Weight;
            }
            set
            {
                _gCost = value;
            }
        }

        private int _gCost;
        public int hCost;
        public int fCost;
        public GridNode parent;
        private byte _obstacleCount;
        public byte ObstacleCount {
            get {

                return _obstacleCount;
            }
        }
        public void UpdateUnwalkable () {
            _unwalkable = this._obstacleCount > 0;
        }
        private bool _unwalkable;
        public bool Unwalkable {
            get {
                return _unwalkable;
            }
        }

        public void AddObstacle () {
            #if DEBUG
            if (this._obstacleCount == byte.MaxValue) {
                Debug.LogErrorFormat("Too many obstacles on this node ({0})!", new Coordinate (this.gridX,this.gridY));
            }
            #endif
            this._obstacleCount++;
            this.UpdateUnwalkable();
        }
        public void RemoveObstacle () {
            if (this._obstacleCount == 0) {
                Debug.LogErrorFormat("No obstacle to remove on this node ({0})!", new Coordinate(this.gridX,this.gridY));
            }
            this._obstacleCount--;
            this.UpdateUnwalkable();
        }

        public bool Unpassable(int size)
        {
            if (this.Unwalkable)
                return true;
            if (size == 1)
            {
                return false;
            }
            if (size <= 3)
            {
                bool unpassable = false;
                for (int i = 0; i < 8; i++)
                {
                    GridNode node = NeighborNodes [i];
                    if (node != null)
                    if (node.Unwalkable)
                        return true;
                }
                return false;
            }
            int deltaCount = GridManager.GenerateDeltaCount((size + 1) / 2);
            //TODO: Optimize delta count generation with data-driven cache
            //Possibility: Split into 3 functions with single place for conditionals?
            for (int i = 1; i < deltaCount; i++)
            {
                GridNode node = GridManager.GetNode(DeltaCache.CacheX [i] + this.gridX, DeltaCache.CacheY [i] + this.gridY);
                if (node.Unwalkable)
                    return true;
                
            }
            return false;
        }

        public int Weight;
        public GridNode[] NeighborNodes = new GridNode[8];
        public Vector2d WorldPos;
        public static readonly bool[] IsNeighborDiagnal = new bool[]
        {
            true,
            false,
            true,
            false,
            false,
            true,
            false,
            true
        };

        private void GenerateNeighbors()
        {
            for (i = -1; i <= 1; i++)
            {
                checkX = gridX + i;
                if (checkX >= 0 && checkX < GridManager.NodeCount)
                {
                    for (j = -1; j <= 1; j++)
                    {
						
                        checkY = gridY + j;
                        if (checkY >= 0 && checkY < GridManager.NodeCount)
                        {
                            GridNode checkNode = GridManager.Grid [GridManager.GetGridIndex(checkX, checkY)];
							
                            if (i == 0 && j == 0)
                                continue;

							
                            //if ((i != 0 && j != 0)) continue;
                            NeighborNodes [GetNeighborIndex(i, j)] = checkNode;
                        }
                    }
                }
            }
			
			
        }

        public static int GetNeighborIndex(int _i, int _j)
        {
            /*
			if (_j == 0) {
				if (_i == -1)
					leIndex = 0;
				else
					leIndex = 3;
			} else {
				if (_j == -1)
					leIndex = 1;
				else
					leIndex = 2;
			}*/
            leIndex = (_i + 1) * 3 + (_j + 1);
            if (leIndex > 3)
                leIndex--;
            return leIndex;
        }

		
        static int dstX;
        static int dstY;
        public static int HeuristicTargetX;
        public static int HeuristicTargetY;

        public void CalculateHeuristic()
        {
            /*
			//Euclidian
			dstX = HeuristicTargetX - gridX;
			dstY = HeuristicTargetY - gridY;
			hCost = (dstX * dstX + dstY * dstY);
			/*if (hCost > 1) {

				n = (hCost / 2) + 1;
				n1 = (n + (hCost / n)) / 2;  
				while (n1 < n) {
					n = n1;  
					n1 = (n + (hCost / n)) / 2;  
				}
				hCost = n;
			}

			fCost = gCost + hCost;
*/
			
            /*
			if (gridX > HeuristicTargetX)
				dstX = gridX - HeuristicTargetX;
			else
				dstX = HeuristicTargetX - gridX;
			if (gridY > HeuristicTargetY)
				dstY = gridY - HeuristicTargetY;
			else
				dstY = HeuristicTargetY - gridY;

			hCost = (dstX + dstY) * 100;
			fCost = gCost + hCost;
			*/
			
            if (gridX > HeuristicTargetX)
                dstX = gridX - HeuristicTargetX;
            else
                dstX = HeuristicTargetX - gridX;
			
            if (gridY > HeuristicTargetY)
                dstY = gridY - HeuristicTargetY;
            else
                dstY = HeuristicTargetY - gridY;
			
            if (dstX > dstY)
                this.hCost = dstY * 141 + (dstX - dstY) * 100;
            else
                this.hCost = dstX * 141 + (dstY - dstX) * 100;
            fCost = gCost + hCost;
			
        }

        #endregion


        #region Influence

        public int ScanX { get { return LinkedScanNode.X; } }

        public int ScanY { get { return LinkedScanNode.Y; } }

        public ScanNode LinkedScanNode;
        const int weightPerUnit = 100;

        public void Add(LSInfluencer influencer)
        {
            //Weight += weightPerUnit;
            LinkedScanNode.Add(influencer);
        }

        public void Remove(LSInfluencer influencer)
        {
            //Weight -= weightPerUnit;
            LinkedScanNode.Remove(influencer);
        }

        #endregion

        static int i, j, checkX, checkY, leIndex;

        public override int GetHashCode()
        {
            return this.gridIndex;
        }

        public bool DoesEqual(GridNode obj)
        {
            return obj.gridIndex == this.gridIndex;
        }

        public override string ToString()
        {
            return "(" + gridX.ToString() + ", " + gridY.ToString() + ")";
        }
    }
}
