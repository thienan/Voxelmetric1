﻿using UnityEngine;
using System;

[Serializable]
public struct BlockPos
{
    public int x, y, z;

    public BlockPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    //Overriding GetHashCode and Equals gives us a faster way to
    //compare two positions and we have to do that a lot
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 47;
            
            hash = hash * 227 + x.GetHashCode();
            hash = hash * 227 + y.GetHashCode();
            hash = hash * 227 + z.GetHashCode();
            return hash * 227;
        }
    }

    public float Random(byte seed, bool extraPrecision = false)
    {
        int hash = GetHashCode();
        unchecked
        {
            hash *= primeNumbers[seed];

            if (extraPrecision)
            {
                hash *= GetHashCode() * primeNumbers[seed++];

                if (hash < 0)
                    hash *= -1;

                return (hash % 10000) / 10000f;
            }

            if (hash < 0)
                hash *= -1;

            return (hash % 100) / 100f;
        }
    }

    public override bool Equals(object obj)
    {
        if (GetHashCode() == obj.GetHashCode())
            return true;

        return false;
    }

    //returns the position of the chunk containing this block
    public BlockPos ContainingChunkCoordinates() {
        int x = Mathf.FloorToInt(this.x / (float)Config.Env.ChunkSize) * Config.Env.ChunkSize;
        int y = Mathf.FloorToInt(this.y / (float)Config.Env.ChunkSize) * Config.Env.ChunkSize;
        int z = Mathf.FloorToInt(this.z / (float)Config.Env.ChunkSize) * Config.Env.ChunkSize;

        return new BlockPos(x, y, z);
    }

    public BlockPos Add(int x, int y, int z)
    {
        return new BlockPos(this.x + x, this.y + y, this.z + z);
    }

    public BlockPos Add(BlockPos pos)
    {
        return new BlockPos(this.x + pos.x, this.y + pos.y, this.z + pos.z);
    }

    public BlockPos Subtract(BlockPos pos)
    {
        return new BlockPos(x - pos.x, y - pos.y, z - pos.z);
    }

    public byte[] ToBytes()
    {
        byte[] BX = BitConverter.GetBytes(x);
        byte[] BY = BitConverter.GetBytes(y);
        byte[] BZ = BitConverter.GetBytes(z);

        return new byte[] {
            BX[0], BX[1], BX[2], BX[3],
            BY[0], BY[1], BY[2], BY[3],
            BZ[0], BZ[1], BZ[2], BZ[3]};
    }

    public static BlockPos FromBytes(byte[] bytes, int offset)
    {
        return new BlockPos(
                    BitConverter.ToInt32(bytes, offset),
                    BitConverter.ToInt32(bytes, offset + 4),
                    BitConverter.ToInt32(bytes, offset + 8));
    }

    //BlockPos and Vector3 can be substituted for one another
    public static implicit operator BlockPos(Vector3 v)
    {
        BlockPos blockPos = new BlockPos(
            Mathf.RoundToInt(v.x / Config.Env.BlockSize),
            Mathf.RoundToInt(v.y / Config.Env.BlockSize),
            Mathf.RoundToInt(v.z / Config.Env.BlockSize)
            );

        return blockPos;
    }

    public static implicit operator Vector3(BlockPos pos)
    {
        return new Vector3(pos.x, pos.y, pos.z) * Config.Env.BlockSize;
    }

    public static implicit operator BlockPos(Direction d)
    {
        switch (d) {
            case Direction.up:
                return new BlockPos(0, 1, 0);
            case Direction.down:
                return new BlockPos(0, -1, 0);
            case Direction.north:
                return new BlockPos(0, 0, 1);
            case Direction.east:
                return new BlockPos(1, 0, 0);
            case Direction.south:
                return new BlockPos(0, 0, -1);
            case Direction.west:
                return new BlockPos(-1, 0, 0);
            default:
                return new BlockPos();
        }
    }

    //These operators let you add and subtract BlockPos from each other
    //or check equality with == and !=
    public static BlockPos operator -(BlockPos pos1, BlockPos pos2)
    {
        return pos1.Subtract(pos2);
    }

    public static bool operator >(BlockPos pos1, BlockPos pos2)
    {
        return (pos1.x > pos2.x || pos1.y > pos2.y || pos1.z > pos2.x);
    }

    public static bool operator <(BlockPos pos1, BlockPos pos2)
    {
        return (pos1.x < pos2.x || pos1.y < pos2.y || pos1.z < pos2.x);
    }

    public static bool operator >=(BlockPos pos1, BlockPos pos2)
    {
        return (pos1.x >= pos2.x || pos1.y >= pos2.y || pos1.z >= pos2.x);
    }

    public static bool operator <=(BlockPos pos1, BlockPos pos2)
    {
        return (pos1.x <= pos2.x || pos1.y <= pos2.y || pos1.z <= pos2.x);
    }

    public static BlockPos operator +(BlockPos pos1, BlockPos pos2)
    {
        return pos1.Add(pos2);
    }

    public static BlockPos operator *(BlockPos pos, int i)
    {
        return new BlockPos(pos.x * i, pos.y * i, pos.z * i);
    }

    public static BlockPos operator *(BlockPos pos1, BlockPos pos2)
    {
        return new BlockPos(pos1.x * pos2.x, pos1.y * pos2.y, pos1.z * pos2.z);
    }

    public static bool operator ==(BlockPos pos1, BlockPos pos2)
    {
        return Equals(pos1, pos2);
    }

    public static bool operator !=(BlockPos pos1, BlockPos pos2)
    {
        return !Equals(pos1, pos2);
    }

    //You can safely use BlockPos as part of a string like this:
    //"block at " + BlockPos + " is broken."
    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    // first 255 prime numbers and 1. Used for randomizing a number in the RandomPercent function.
    static readonly int[] primeNumbers = new int[]{1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619};
}