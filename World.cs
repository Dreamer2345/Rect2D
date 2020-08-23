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
            for(int i = 0; i < PhysicsRectangles.Count; i++)
            {
                for (int j = 0; j < PhysicsRectangles.Count; j++)
                {
                    if(i != j)
                    {
                        PhysicsRectangle a = PhysicsRectangles[i];
                        PhysicsRectangle b = PhysicsRectangles[j];

                        a.X = a.X + (a.VelX * Delta);
                        a.Y = a.Y + (a.VelY * Delta);

                        b.X = b.X + (b.VelX * Delta);
                        b.Y = b.Y + (b.VelY * Delta);

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
                    PhysicsRectangles[PossibleContacts[i].Item2] = b;
                    ActualContacts.Add(c);
                }
                else
                {
                    PossibleContacts.RemoveAt(i);
                }
            }
        }

        bool ContainsRectangle(int colID)
        {
            for (int i = 0; i < PossibleContacts.Count; i++)
            {
                if ((PossibleContacts[i].Item1 == colID) || ((PossibleContacts[i].Item2 == colID)))
                    return true;

            }
            return false;
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

                    if (ContainsRectangle(i))
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
