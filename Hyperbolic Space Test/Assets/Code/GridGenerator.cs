using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GridGenerator : MonoBehaviour
{
    [Serializable]
    private class SpecialTile
    {
        public string Coord;
        public SymbolicCoordinate TilePrefab;
    }
    
    [SerializeField] private SymbolicCoordinate _defaultTilePrefab = null;
    [SerializeField] private SpecialTile[] _tiles = null;
    
    [Space, SerializeField] private int _depth = 4;
    [SerializeField] private string[] _tilesToDestroy = null;
    
    private List<SymbolicCoordinate> _generatedTiles = new List<SymbolicCoordinate>();
    
    private void Start()
    {
        foreach (var tile in _tiles)
        {
            Instantiate(tile.TilePrefab).SetSymbolicCoordinate(tile.Coord);
        }

        if (_depth != 0)
        {
            GenerateTiles();
        }
    }
    
    void GenerateTiles()
    {
        string[] possibleMoves = {"u", "ru", "lu", "rru"};

        Queue<string> generatedCoordinates = new Queue<string>();
        List<string> coordinatesToGenerate = new List<string>();
        
        generatedCoordinates.Enqueue("");
        while (generatedCoordinates.Count != 0)
        {
            string currentCoordinate = generatedCoordinates.Dequeue();

            coordinatesToGenerate.Add(currentCoordinate);
            //coordinatesToGenerate = coordinatesToGenerate.Distinct(new DistinctItemComparer()).ToList();
            
            if (currentCoordinate.Count(c => c == 'u') >= _depth)
            {
                continue;
            }

            for (var index = 0; index < possibleMoves.Length; index++)
            {
                string newCoord = currentCoordinate + possibleMoves[index];

                string reducedCoordinate = SquareCoordinateReducer.ReduceCoordinate(newCoord);

                if (coordinatesToGenerate.Exists(coord => string.CompareOrdinal(coord, reducedCoordinate) == 0))
                {
                    continue;
                }
                
                generatedCoordinates.Enqueue(reducedCoordinate);
            }
        }
        
        

        int counter = 0;
        foreach (var coordinate in coordinatesToGenerate)
        {
            if (_tilesToDestroy.Contains(coordinate, new StringComparer()))
            {
                continue;
            }
            counter += 1;
            SymbolicCoordinate tile = Instantiate(_defaultTilePrefab);
            tile.name = "Tile " + coordinate;
            tile.SetSymbolicCoordinate(coordinate);
        }
    }
    
    class StringComparer : IEqualityComparer<string> {

        public bool Equals(string x, string y) {
            return string.CompareOrdinal(x, y) == 0;
        }

        public int GetHashCode(string obj) {
            return obj.GetHashCode();
        }
    }
}