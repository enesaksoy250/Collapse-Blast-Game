using System.Collections.Generic;
using UnityEngine;

public interface IGroupFinder
{
    List<Vector2Int> FindGroup(int row, int column);
    List<List<Vector2Int>> FindAllGroups();
    bool HasValidGroup();
}
