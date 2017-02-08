#region Using directives

using System;

#endregion

namespace AvanteSales.SystemFramework
{
    public class TypedHashtable
    {
        /// <summary>
        /// Classe que representa o item da tabela hash
        /// </summary>
        public class TypedHashtableEntry
        {
            private int hash;
            private string key;
            private object value;
            private HashtableEntryType type;
            private TypedHashtableEntry next;

            // [ Código hash do item ]
            public int Hash
            {
                get
                {
                    return this.hash;
                }
                set
                {
                    this.hash = value;
                }
            }

            // [ Chave de identificação do item ]
            public string Key
            {
                get
                {
                    return this.key;
                }
                set
                {
                    this.key = value;
                }
            }

            // [ Valor do item ]
            public object Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                }
            }

            // [ Identificação do tipo do item ]
            public HashtableEntryType Type
            {
                get
                {
                    return this.type;
                }
                set
                {
                    this.type = value;
                }
            }

            // [ Indica o próximo item com mesmo código hash ]
            public TypedHashtableEntry Next
            {
                get
                {
                    return this.next;
                }
                set
                {
                    this.next = value;
                }
            }
        }

        private TypedHashtableEntry[] table;
        private int count;
        private int threshold;
        private const int loadFactorPercent = 75;

        public enum HashtableEntryType
        {
            All,
            Permanent,
            Temporary
        }

        public TypedHashtable(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentException();
            }
            if (initialCapacity == 0)
            {
                initialCapacity = 1;
            }
            table = new TypedHashtableEntry[initialCapacity];
            threshold = (initialCapacity * 75) / 100;
            count = 0;
        }

        public int Size()
        {
            return count;
        }

        public object Get(string key)
        {
            TypedHashtableEntry[] tab = table;
            int hash = GetHashCode(key);
            int index = (hash & 0x7fffffff) % tab.Length;

            for (TypedHashtableEntry e = tab[index]; e != null; e = e.Next)
            {
                if (e.Hash == hash && e.Key == key)
                {
                    return e.Value;
                }
            }

            return null;
        }

        private void Rehash()
        {
            int oldCapacity = table.Length;
            TypedHashtableEntry[] oldTable = table;
            int newCapacity = oldCapacity * 2 + 1;
            TypedHashtableEntry[] newTable = new TypedHashtableEntry[newCapacity];

            threshold = (newCapacity * 75) / 100;
            table = newTable;

            for (int i = oldCapacity; i-- > 0; )
            {
                TypedHashtableEntry old = oldTable[i];

                while (old != null)
                {
                    TypedHashtableEntry e = old;
                    old = old.Next;
                    int index = (e.Hash & 0x7fffffff) % newCapacity;
                    e.Next = newTable[index];
                    newTable[index] = e;
                }
            }

        }

        public object Put(string key, object value, HashtableEntryType type)
        {
            if (value == null)
                throw new NullReferenceException();

            TypedHashtableEntry[] tab = table;

            int hash = GetHashCode(key);
            int index = (hash & 0x7fffffff) % tab.Length;

            for (TypedHashtableEntry e = tab[index]; e != null; e = e.Next)
            {
                if (e.Hash == hash && e.Key == key)
                {
                    object old = e.Value;
                    e.Value = value;
                    e.Type = type;
                    return old;
                }
            }

            if (count >= threshold)
            {
                Rehash();
                return Put(key, value, type);

            }
            else
            {
                TypedHashtableEntry e = new TypedHashtableEntry();

                e.Hash = hash;
                e.Key = key;
                e.Value = value;
                e.Type = type;
                e.Next = tab[index];
                tab[index] = e;

                count++;

                return null;
            }
        }

        public int Clear(HashtableEntryType type)
        {
            int total = count;
            TypedHashtableEntry[] tab = table;

            for (int index = tab.Length; --index >= 0; )
            {
                TypedHashtableEntry e = tab[index];
                TypedHashtableEntry prev = null;

                for (; e != null; e = e.Next)
                {
                    if (type == HashtableEntryType.All || type == e.Type)
                    {
                        if (e.Value is IDisposable)
                            ((IDisposable)e.Value).Dispose();

                        if (prev != null)
                        {
                            prev.Next = e.Next;
                        }
                        else
                        {
                            tab[index] = e.Next;
                        }
                        count--;

                    }
                    else
                    {
                        prev = e;
                    }
                }
            }

            // [ Retorna o total de itens descartados ]
            return total - count;
        }

        public int CountCollisions()
        {
            TypedHashtableEntry[] tab = table;
            int collisions = 0;

            for (int index = tab.Length; --index >= 0; )
            {
                TypedHashtableEntry e = tab[index];

                for (; e != null; e = e.Next)
                {
                    if (e.Next != null)
                        collisions++;
                }
            }

            return collisions;
        }

        public System.Collections.ArrayList ListCollisions()
        {
            TypedHashtableEntry[] tab = table;
            System.Collections.ArrayList collisions = new System.Collections.ArrayList();

            for (int index = tab.Length; --index >= 0; )
            {
                TypedHashtableEntry e = tab[index];

                string keys = "";
                for (; e != null; e = e.Next)
                {
                    if (e.Next != null)
                        keys += e.Next.Key + "|";
                }

                if (keys.Length > 0)
                {
                    keys += tab[index].Key;
                    collisions.Add(keys);
                }
            }

            return collisions;
        }

        public System.Collections.ArrayList ListElements()
        {
            TypedHashtableEntry[] tab = table;
            System.Collections.ArrayList elements = new System.Collections.ArrayList();

            for (int index = tab.Length; --index >= 0; )
            {
                TypedHashtableEntry e = tab[index];

                for (; e != null; e = e.Next)
                    elements.Add(e.Key);
            }

            return elements;
        }

        private int GetHashCode(string key)
        {
            return HashFunctionLibrary.SDBMHash(key);
        }
    }
}