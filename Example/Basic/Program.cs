using Rect2D;
using System;
using Utils;

namespace Rect2DTesting
{
    class Program
    {
     
        static void PrintRectangle(int index,PhysicsRectangle a)
        {
            Console.WriteLine($"===============\nIndex:{index}\nX:{a.X} Y:{a.Y}\nSizeX:{a.SizeX} SizeY:{a.SizeY}\nDX:{a.VelX} DY:{a.VelY} Parent:{a.Parent} Hit: Up: {a.Hit[(int)Direction.Up]} Down: {a.Hit[(int)Direction.Down]} Left: {a.Hit[(int)Direction.Left]} Up: {a.Hit[(int)Direction.Right]}\n===============");
        }

        static void Main(string[] args)
        {
            Rect2DWorld physicsWorld = new Rect2DWorld();
            physicsWorld.GravityY = 1;
            int Box1 = physicsWorld.AddRectangle(new PhysicsRectangle() { X = 0, Y = 0, SizeX = 10, SizeY = 10, VelX = 0, VelY = 0, Mass = 1, Friction = 0.98f, Parent = 0 });
            int Box2 = physicsWorld.AddRectangle(new PhysicsRectangle() { X = 0, Y = 20, SizeX = 10, SizeY = 10, VelX = 0, VelY = 0 , Mass = -1, Parent = 1});
            
            while (true)
            {
                Console.WriteLine("===========================\n");
                physicsWorld.Update(1);

                foreach(Contact i in physicsWorld.GetContacts())
                {
                    Console.WriteLine($"Contact: {i.Collided} Position: X:{i.X} Y:{i.Y} Normal: X:{i.NX} Y:{i.NY} TimeToContact: {i.TimeOfContact}");
                }

                PrintRectangle(Box1, physicsWorld.GetRectangle(Box1));
                PrintRectangle(Box2, physicsWorld.GetRectangle(Box2));
                string str = Console.ReadLine();
                if(str != "")
                {
                    physicsWorld.UpdateRectangle(new PhysicsRectangle() { X = 0, Y = 0, SizeX = 10, SizeY = 10, VelX = 0, VelY = 0, Mass = 1, Friction = 0.98f, Parent = 0 }, Box1);
                    physicsWorld.UpdateRectangle(new PhysicsRectangle() { X = 0, Y = 20, SizeX = 10, SizeY = 10, VelX = 0, VelY = 0, Mass = -1, Parent = 1 }, Box2);
                }
            }
        }
    }
}
