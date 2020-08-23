using Rect2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{

    public struct Contact
    {
        public bool Collided;
        public float X, Y, NX, NY, TimeOfContact;
    }

    class CollisionUtils
    {

        static void Swap(ref float x, ref float y)
        {
            float s = x;
            x = y;
            y = s;
        }

        

        public static bool RectangleIntersect(PhysicsRectangle r, PhysicsRectangle r1)
        {
            return (r.X < r1.X + r1.SizeX) && (r.X + r.SizeX > r1.X) && (r.Y < r1.Y + r1.SizeY) && (r.Y + r.SizeY > r1.Y);
        }

        public static Contact RectangleVSRay(PhysicsRectangle r, Ray l)
        {

            float inverseX = 1.0f / l.dx;
            float inverseY = 1.0f / l.dy;

            float NearX = (r.X - l.x) * inverseX;
            float NearY = (r.Y - l.y) * inverseY;

            float FarX = ((r.X + r.SizeX) - l.x) * inverseX;
            float FarY = ((r.Y + r.SizeY) - l.y) * inverseY;

            //Console.WriteLine("X:" + inverseX + " Y:" + inverseY);

            //Console.WriteLine("X:"+(r.X - l.x) + "::"+ ((r.X + r.SizeX) - l.x));
            //Console.WriteLine("Y:"+(r.Y - l.y) + "::" + ((r.Y + r.SizeY) - l.y));

            //Console.WriteLine("X:" + NearX + "::" + FarX);
            //Console.WriteLine("Y:" + NearY + "::" + FarY);

            if (float.IsNaN(NearX) || float.IsNaN(NearY)) return new Contact { Collided = false };
            if (float.IsNaN(FarX) || float.IsNaN(FarY)) return new Contact { Collided = false };

            //Console.WriteLine("C1");

            if (NearX > FarX) Swap(ref NearX, ref FarX);
            if (NearY > FarY) Swap(ref NearY, ref FarY);

            if ((NearX > FarY) || (NearY > FarX)) return new Contact { Collided = false };

            float TimeHitNear = Math.Max(NearX, NearY);
            float TimeHitFar = Math.Min(FarX, FarY);

            if (TimeHitFar < 0) return new Contact { Collided = false };

            float ContactX = l.x + (l.dx * TimeHitNear);
            float ContactY = l.y + (l.dy * TimeHitNear);


            float NormalX = 0;
            float NormalY = 0;

            if(NearX > NearY)
            {
                if (l.dx < 0)
                {
                    NormalX = 1;
                }
                else
                {
                    NormalX = -1;
                }
            }
            else if(NearX < NearY)
            {
                if (l.dy < 0)
                {
                    NormalY = 1;
                }
                else
                {
                    NormalY = -1;
                }
            }
            //Console.WriteLine(TimeHitNear + ":" + TimeHitFar);

            if((TimeHitNear >= 0.0f)&&(TimeHitNear < 1.0f))
                return new Contact() { Collided = true, NX = NormalX, NY = NormalY, TimeOfContact = TimeHitNear, X = ContactX, Y = ContactY };
            else
                return new Contact() { Collided = false, NX = NormalX, NY = NormalY, TimeOfContact = TimeHitNear, X = ContactX, Y = ContactY };

        }

        public static Contact RectangleVSRectangle(PhysicsRectangle r, PhysicsRectangle r1, float Delta)
        {
            if ((r.VelX == 0) && (r.VelY == 0))
                return new Contact() { Collided = false };

            PhysicsRectangle Expanded = new PhysicsRectangle() { SizeX = r1.SizeX + r.SizeX, 
                                                                 SizeY = r1.SizeY + r.SizeY, 
                                                                 X = r1.X - (r.SizeX / 2), 
                                                                 Y = r1.Y - (r.SizeY / 2) 
                                                               };
            Ray ray = new Ray() { x = r.X + (r.SizeX / 2), y = r.Y + (r.SizeY / 2) , dx = r.VelX * Delta, dy = r.VelY * Delta};

            //Console.WriteLine($"X:{Expanded.X} Y:{Expanded.Y}\nSizeX:{Expanded.SizeX} SizeY:{Expanded.SizeY}");

            return RectangleVSRay(Expanded, ray);
        }
    
    }
}
