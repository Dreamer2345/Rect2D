using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rect2D
{

    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public struct PhysicsRectangle
    {
        public object Parent;

        public object[] Hit;
        public float X, Y, VelX, VelY, SizeX, SizeY, Mass,Bounce, Friction;

        public PhysicsRectangle(object Parent)
        {
            this.Parent = Parent;
            X = 0;
            Y = 0;
            VelX = 0;
            VelY = 0;
            SizeX = 0;
            SizeY = 0;
            Mass = -1;
            Bounce = 0;
            Friction = 0;

            Hit = new object[4];
        }

        public PhysicsRectangle Clone()
        {
            return (PhysicsRectangle)this.MemberwiseClone();
        }

    }
}
