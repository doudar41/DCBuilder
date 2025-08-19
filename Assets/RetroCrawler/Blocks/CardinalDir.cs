using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public enum CardinalDirections
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}

public static class CardinalDir
{
    public static CardinalDirections GetOpposite(CardinalDirections dir)
    {
        CardinalDirections dirOpposite = new CardinalDirections();
        switch (dir)
        {
            case CardinalDirections.EAST:
                dirOpposite = CardinalDirections.WEST;
                break;
            case CardinalDirections.SOUTH:
                dirOpposite = CardinalDirections.NORTH;
                break;
            case CardinalDirections.WEST:
                dirOpposite = CardinalDirections.EAST;
                break;
            case CardinalDirections.NORTH:
                dirOpposite = CardinalDirections.SOUTH;
                break;
        }
        return dirOpposite;
    }

    public static Vector3 GetNewPoint(CardinalDirections currentdir, Vector3Int startPosition, Tilemap tilemap)
    {
        Vector3 v = new Vector3();
        Vector3Int dir;
        switch (currentdir)
        {
            case CardinalDirections.EAST:
                dir = new Vector3Int(startPosition.x + 1, startPosition.y);
                v = tilemap.GetCellCenterWorld(dir);
                break;
            case CardinalDirections.SOUTH:
                dir = new Vector3Int(startPosition.x, startPosition.y - 1);
                v = tilemap.GetCellCenterWorld(dir);
                break;
            case CardinalDirections.WEST:
                dir = new Vector3Int(startPosition.x - 1, startPosition.y);
                v = tilemap.GetCellCenterWorld(dir);
                break;
            case CardinalDirections.NORTH:
                dir = new Vector3Int(startPosition.x, startPosition.y + 1);
                v = tilemap.GetCellCenterWorld(dir);
                break;
        }
        return v;
    }


    public static CardinalDirections GetRightDir(CardinalDirections dir)
    {
        CardinalDirections dirOpposite = new CardinalDirections();
        switch (dir)
        {
            case CardinalDirections.EAST:
                dirOpposite = CardinalDirections.SOUTH;
                break;
            case CardinalDirections.SOUTH:
                dirOpposite = CardinalDirections.WEST;
                break;
            case CardinalDirections.WEST:
                dirOpposite = CardinalDirections.NORTH;
                break;
            case CardinalDirections.NORTH:
                dirOpposite = CardinalDirections.EAST;
                break;
        }

        return dirOpposite;
    }


    public static CardinalDirections SetDirectionRight(CardinalDirections currentdir)
    {
        CardinalDirections dirRight = new CardinalDirections();

        switch (currentdir)
        {
            case CardinalDirections.EAST:

                dirRight = CardinalDirections.SOUTH;
                break;
            case CardinalDirections.SOUTH:

                dirRight = CardinalDirections.WEST;
                break;
            case CardinalDirections.WEST:

                dirRight = CardinalDirections.NORTH;
                break;
            case CardinalDirections.NORTH:
                dirRight = CardinalDirections.EAST;
                break;
        }
        return dirRight;
    }

    public static CardinalDirections SetDirectionLeft(CardinalDirections currentdir)
    {
        CardinalDirections dirRight = new CardinalDirections();

        switch (currentdir)
        {
            case CardinalDirections.EAST:

                dirRight = CardinalDirections.NORTH;
                break;
            case CardinalDirections.SOUTH:

                dirRight = CardinalDirections.EAST;
                break;
            case CardinalDirections.WEST:

                dirRight = CardinalDirections.SOUTH;
                break;
            case CardinalDirections.NORTH:
                dirRight = CardinalDirections.WEST;
                break;
        }
        return dirRight;
    }

    public static Vector3 GetForwardVectorFromDirection(CardinalDirections currentdir)
    {
        Vector3 frw= new Vector3();

        switch (currentdir)
        {
            case CardinalDirections.EAST:

                frw = Vector3.right;
                break;
            case CardinalDirections.SOUTH:

                frw = Vector3.back;
                break;
            case CardinalDirections.WEST:

                frw = Vector3.left;
                break;
            case CardinalDirections.NORTH:
                frw = Vector3.forward;
                break;
        }
        return frw;
    }


    public static CardinalDirections GetDirectionFromNormVector(Vector3 currentdir, CardinalDirections cardinal)
    {
        CardinalDirections dirRight = new CardinalDirections();


        if (currentdir == Vector3.forward) 
        {
            switch (cardinal)
            {
                case CardinalDirections.NORTH:
                    dirRight = CardinalDirections.NORTH;
                    break;
                case CardinalDirections.EAST:
                    dirRight = CardinalDirections.EAST;
                    break;
                case CardinalDirections.SOUTH:
                    dirRight = CardinalDirections.SOUTH;
                    break;
                case CardinalDirections.WEST:
                    dirRight = CardinalDirections.WEST;
                    break;
            }
            
            return dirRight;
        }
        if (currentdir == Vector3.back)
        {
            switch (cardinal)
            {
                case CardinalDirections.NORTH:
                    dirRight = CardinalDirections.SOUTH;
                    break;
                case CardinalDirections.EAST:
                    dirRight = CardinalDirections.WEST;
                    break;
                case CardinalDirections.SOUTH:
                    dirRight = CardinalDirections.NORTH;
                    break;
                case CardinalDirections.WEST:
                    dirRight = CardinalDirections.EAST;
                    break;
            }

            return dirRight;
        }
        if (currentdir == Vector3.right)
        {
            switch (cardinal)
            {
                case CardinalDirections.NORTH:
                    dirRight = CardinalDirections.EAST;
                    break;
                case CardinalDirections.EAST:
                    dirRight = CardinalDirections.SOUTH;
                    break;
                case CardinalDirections.SOUTH:
                    dirRight = CardinalDirections.WEST;
                    break;
                case CardinalDirections.WEST:
                    dirRight = CardinalDirections.NORTH;
                    break;
            }

            return dirRight;
        }
        if (currentdir == Vector3.left)
        {
            switch (cardinal)
            {
                case CardinalDirections.NORTH:
                    dirRight = CardinalDirections.WEST;
                    break;
                case CardinalDirections.EAST:
                    dirRight = CardinalDirections.NORTH;
                    break;
                case CardinalDirections.SOUTH:
                    dirRight = CardinalDirections.EAST;
                    break;
                case CardinalDirections.WEST:
                    dirRight = CardinalDirections.SOUTH;
                    break;
            }
            return dirRight;
        }
        return dirRight;
    }



    public static float GetRotationYForCardinal(CardinalDirections cardinal)
    {
        switch (cardinal)
        {
            case CardinalDirections.NORTH:
                return 0;
            case CardinalDirections.EAST:
                return 90;
            case CardinalDirections.SOUTH:
                return 180;
            case CardinalDirections.WEST:
                return 270;
        }
        return 0;
    }

    public static float FindClosestYRotation(float angle)
    {
        float y = 0;
        y = angle % 360;
        if (y > 315 && y <= 45) return 0;
        if (y > 45 && y <= 135) return 90;
        if (y > 135 && y <= 225) return 180;
        if (y > 225 && y <= 315) return 270;
        return 0;
    }


    public static List<OnBlockPlacement> GetBlockAround(OnBlockPlacement currentBlock, Tilemap tilemap)
    {
        List<OnBlockPlacement> blocksAround = new List<OnBlockPlacement>();
        return blocksAround;

    }


}


