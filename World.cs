using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Rect2D
{
    public class World
    {
        public float GravityX = 0;
        public float GravityY = 1;

        GrowList<PhysicsRectangle> PhysicsRectangles = new GrowList<PhysicsRectangle>();
        List<Tuple<int, int>> PossibleContacts = new List<Tuple<int , int>>();
        List<Contact> ActualContacts = new List<Contact>();
        public int AddRectangle(PhysicsRectangle a)
        {
            if (a.Hit == null)
                a.Hit = new object[4];
            return PhysicsRectangles.Add(a);
        }

        public void RemoveRectangle(int a)
        {
            PhysicsRectangles.Recycle(a);
        }

        public void UpdateRectangle(PhysicsRectangle a, int Id)
        {
            PhysicsRectangles[Id] = a;
        }

        public PhysicsRectangle GetRectangle(int a)
        {
            return PhysicsRectangles[a];
        }

        public PhysicsRectangle[] GetRectangles()
        {
            return PhysicsRectangles.ToArray();
        }

        public Contact[] GetContacts()
        {
            return ActualContacts.ToArray();
        }

        void BroadPhase(float Delta)
        {
            PossibleContacts.Clear();
            ActualContacts.Clear();

            //for (int i = 0; i < PhysicsRectangles.Count; i++)
            //{
            //    PhysicsRectangle a = PhysicsRectangles[i];
            //    for (int j = 0; j < 4; i++)
            //    {
            //        a.Hit[j] = null;
            //    }
            //    PhysicsRectangles[i] = a;
            //}

            for (int i = 0; i < PhysicsRectangles.Count; i++)
            {
                for (int j = 0; j < PhysicsRectangles.Count; j++)
                {
                    if(i != j)
                    {
                        PhysicsRectangle a = PhysicsRectangles[i];
                        PhysicsRectangle b = PhysicsRectangles[j];

                        float AMinX = Math.Min(a.X, a.X + (a.VelX * Delta));
                        float AMinY = Math.Min(a.Y, a.Y + (a.VelY * Delta));

                        float AMaxX = Math.Max(a.X + a.SizeX, a.X + a.SizeX + (a.VelX * Delta));
                        float AMaxY = Math.Max(a.Y + a.SizeY, a.Y + a.SizeY + (a.VelY * Delta));

                        float BMinX = Math.Min(b.X, b.X + (b.VelX * Delta));
                        float BMinY = Math.Min(b.Y, b.Y + (b.VelY * Delta));

                        float BMaxX = Math.Max(b.X + b.SizeX, b.X + b.SizeX + (b.VelX * Delta));
                        float BMaxY = Math.Max(b.Y + b.SizeY, b.Y + b.SizeY + (b.VelY * Delta));

                        a.SizeX = Math.Abs(AMaxX - AMinX);
                        a.SizeY = Math.Abs(AMaxY - AMinY);
                        a.X = AMinX;
                        a.Y = AMinY;

                        b.SizeX = Math.Abs(BMaxX - BMinX);
                        b.SizeY = Math.Abs(BMaxY - BMinY);
                        b.X = BMinX;
                        b.Y = BMinY;


                        if (CollisionUtils.RectangleIntersect(a, b))
                        {
                            PossibleContacts.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }
        }

        void NarrowPhase(float Delta)
        {
            for(int i = 0; i < PossibleContacts.Count; i++)
            {
                PhysicsRectangle a = PhysicsRectangles[PossibleContacts[i].Item1];
                PhysicsRectangle b = PhysicsRectangles[PossibleContacts[i].Item2];

                Contact c = CollisionUtils.RectangleVSRectangle(a, b, Delta);

                //Console.WriteLine($"Contact: {c.Collided} Normal: x:{c.NX} y:{c.NY} ContactPoint: x:{c.X} y:{c.Y} ContactTime: {c.TimeOfContact}");

                
                if (c.Collided)
                {
                    if (c.TimeOfContact >= 1)
                        continue;

                    if (c.NX < 0)
                        a.Hit[(int)Direction.Right] = PhysicsRectangles[PossibleContacts[i].Item2].Parent;
                    else
                        a.Hit[(int)Direction.Right] = null;

                    if (c.NX > 0)
                        a.Hit[(int)Direction.Left] = PhysicsRectangles[PossibleContacts[i].Item2].Parent;
                    else
                        a.Hit[(int)Direction.Left] = null;

                    if (c.NY < 0)
                        a.Hit[(int)Direction.Down] = PhysicsRectangles[PossibleContacts[i].Item2].Parent;
                    else
                        a.Hit[(int)Direction.Down] = null;

                    if (c.NY > 0)
                        a.Hit[(int)Direction.Up] = PhysicsRectangles[PossibleContacts[i].Item2].Parent;
                    else
                        a.Hit[(int)Direction.Up] = null;



                    a.VelX += c.NX * Math.Abs(a.VelX) * (1 - c.TimeOfContact);
                    a.VelY += c.NY * Math.Abs(a.VelY) * (1 - c.TimeOfContact);

                    PhysicsRectangles[PossibleContacts[i].Item1] = a;
                    ActualContacts.Add(c);
                }
                else
                {
                    PossibleContacts.RemoveAt(i);
                }
            }
        }

        void UpdatePosition(float Delta)
        {
            for (int i = 0; i < PhysicsRectangles.Count; i++)
            {
                PhysicsRectangle a = PhysicsRectangles[i];
                if (a.Mass > 0)
                {
                    a.X = a.X + (a.VelX * Delta);
                    a.Y = a.Y + (a.VelY * Delta);

                    if (a.Hit[(int)Direction.Down] == null)
                    {
                        a.VelX += (GravityX * Delta);
                        a.VelY += (GravityY * Delta);
                    }
                    else
                    {
                        a.VelX *= a.Friction;
                        a.VelY *= a.Friction;
                    }
                }
                PhysicsRectangles[i] = a;
            }
        }

        public void Update(float Delta)
        {
            BroadPhase(Delta);
            NarrowPhase(Delta);
            UpdatePosition(Delta);
        }
    }
}
