using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public sealed class GrowListElement<T>
    {
        public T Data;
        bool Active = false;

        public bool IsActive { get { return Active; } set { Active = value; } }

        public GrowListElement(T Data)
        {
            this.Data = Data;
        }
    }

    public class GrowList<T> : IEnumerable, IEnumerable<T>
    {
        bool OverideData = false;
        const int BaseCapSize = 100;
        int currentSize;
        int ArrayAllocIncrement { get; set; } = 100;
        List<int> FreeValues = new List<int>();
        List<int> TakenValues = new List<int>();
        GrowListElement<T>[] Data;

        public int CurrentSize { get => currentSize; }

        /*Array Helper Functions*/
        private bool CheckIndexCorrect(int Index, bool ReadWrite = true)
        {
            if ((Index < 0) || (Index > CurrentSize))
            {
                throw new IndexOutOfRangeException("Index for Array was out of range Range[0:" + CurrentSize + "] Got [" + Index + "]");
            }

            if (!Data[Index].IsActive && ReadWrite)
            {
                throw new ArgumentException("Index for object in array was inactive or destroyed?");
            }

            return true;
        }
        private void FreeIndex(int Value, bool Overide = false)
        {
            if (Overide || (!Data[Value].IsActive && TakenValues.Contains(Value)))
            {
                FreeValues.Add(Value);
                TakenValues.Remove(Value);
            }
        }
        private void FreeIndexes(int StartVal, int EndVal, bool Overide = false)
        {
            for (int i = StartVal; i < EndVal; i++)
            {
                FreeIndex(i, Overide);
            }
        }
        private void GrowArray(int NumberOfElements)
        {
            int NewSize = (int)(NumberOfElements + CurrentSize);

            if (NewSize > int.MaxValue)
            {
                NewSize = int.MaxValue;
            }

            if (CurrentSize == (int)NewSize)
            {
                return;
            }

            Array.Resize(ref Data, (int)NewSize);
            FreeIndexes(CurrentSize, (int)NewSize, true);
            currentSize = (int)NewSize;
        }

        /*End Array Helper Functions*/

        /*Functions for interacting with the growList*/
        public T[] ToArray()
        {
            T[] DatArrayOut = new T[TakenValues.Count];
            for (int i = 0; i < TakenValues.Count; i++)
                DatArrayOut[i] = Data[TakenValues[i]].Data;
            return DatArrayOut;
        }
        
        public int Count
        {
            get => TakenValues.Count;
        }
        public T this[int index]
        {
            get
            {
                return GetValue(index);
            }
            set
            {
                SetValue(index, value);
            }
        }
        public void Clear()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                RemoveIndex(i);
            }

            FreeIndexes(0, CurrentSize);
            TakenValues.Clear();
        }
        public void SetValue(int Index, T value)
        {
            CheckIndexCorrect(Index);
            Data[Index].Data = value;
        }
        public T GetValue(int Index)
        {
            CheckIndexCorrect(Index);
            return Data[Index].Data;
        }
        public void Recycle(int Index)
        {
            CheckIndexCorrect(Index, false);
            RemoveIndex(Index);
            FreeIndex(Index);
        }
        public int Add(T Object)
        {
            if (FreeValues.Count == 0)
            {
                GrowArray(ArrayAllocIncrement);
            }

            int NewIndex = FreeValues[0];
            Data[NewIndex] = new GrowListElement<T>(Object) { IsActive = true };
            FreeValues.RemoveAt(0);
            TakenValues.Add(NewIndex);
            return NewIndex;
        }
        public void RemoveIndex(int Index)
        {
            CheckIndexCorrect(Index,false);
            Data[Index].IsActive = false;
            if (OverideData)
            {
                Data[Index].Data = default;
            }

            FreeIndex(Index);
        }       
        public void Remove(T Object)
        {
            Contains(Object, out int Index);
            RemoveIndex(Index);
        }
        public bool Contains(T Element)
        {
            return Contains(Element, out int i);
        }
        public bool Contains(T Element, out int Index)
        {
            foreach (int i in TakenValues)
            {
                if (Element.Equals(Data[i].Data))
                {
                    Index = i;
                    return true;
                }
            }
            Index = 0;
            return false;
        }
        public bool Has(Predicate<T> Where)
        {
            for (int i = 0; i < TakenValues.Count; i++)
            {
                if (Where.Invoke(Data[TakenValues[i]].Data))
                {
                    return true;
                }
            }

            return false;
        }
        public T Find(Predicate<T> Where)
        {
            for (int i = 0; i < TakenValues.Count; i++)
            {
                if (Where.Invoke(Data[TakenValues[i]].Data))
                {
                    return Data[TakenValues[i]].Data;
                }
            }

            return default;
        }
        public int FindIndex(Predicate<T> Where)
        {
            for (int i = 0; i < TakenValues.Count; i++)
            {
                if (Where.Invoke(Data[TakenValues[i]].Data))
                {
                    return i;
                }
            }

            return -1;
        }

        
        /*Initialization functions*/
        public GrowList()
        {
            Data = new GrowListElement<T>[BaseCapSize];
            currentSize = Data.Length;
            FreeIndexes(0, currentSize, true);
        }
        public GrowList(int ArrayCapSize)
        {
            Data = new GrowListElement<T>[ArrayCapSize];
            currentSize = Data.Length;
            FreeIndexes(0, currentSize, true);
        }
        public GrowList(int ArrayCapSize, int ArrayMemAllocSize)
        {
            ArrayAllocIncrement = ArrayMemAllocSize;
            Data = new GrowListElement<T>[ArrayCapSize];
            currentSize = Data.Length;
            FreeIndexes(0, CurrentSize, true);
        }
        ~GrowList()
        {
            FreeValues.Clear();
        }


        /*Enumarator handling*/
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public GrowlistEnumerator GetEnumerator()
        {
            return new GrowlistEnumerator(Data, TakenValues.ToArray());
        }
        public class GrowlistEnumerator : IEnumerator<T>, IEnumerator
        {
            public T Current
            {
                get
                {
                    return Elements[DataLocations[CurrentIndex]].Data;
                }
            }

            T IEnumerator<T>.Current => Current;
            object IEnumerator.Current => Current; 

            int CurrentIndex = -1;
            GrowListElement<T>[] Elements;
            int[] DataLocations;

            public GrowlistEnumerator(GrowListElement<T>[] listElements, int[] DataLocations)
            {
                CurrentIndex = -1;
                Elements = listElements;
                this.DataLocations = DataLocations;
            }

            public bool MoveNext()
            {
                CurrentIndex++;
                return (CurrentIndex < DataLocations.Length);
            }

            public void Reset()
            {
                CurrentIndex = 0;
            }

            public void Dispose()
            {

            }
        }

    }
}
