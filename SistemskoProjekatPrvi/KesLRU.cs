using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemskoProjekatPrvi
{
    public class KesLRU
    {
        private readonly int m_kapacitet;
        private int m_brojZapisa = 0;
        private LinkedList<String> m_lista;

        private readonly Dictionary<String, String> m_kes;
        public KesLRU(int kapacitet)
        {
            m_kapacitet = kapacitet;
            m_kes = [];
            m_lista = new LinkedList<String>();
        }

        public bool Citaj(String kljuc, out String vrednost)
        {
            if (m_kes.TryGetValue(kljuc, out String? value))
            {
                m_lista.Remove(kljuc);
                m_lista.AddFirst(kljuc);

                vrednost = value;
                return true;
            }

            vrednost = "";
            return false;
        }

        public void Upisi(String kljuc, String vrednost)
        {
            if (m_brojZapisa < m_kapacitet)
            {
                m_lista.AddFirst(kljuc);
                m_kes[kljuc] = vrednost;
                m_brojZapisa++;
            }
            else
            {
                var lru = m_lista.Last;

                if (lru != null)
                {
                    m_lista.RemoveLast();
                    m_lista.AddFirst(kljuc);
                    m_kes.Remove(lru.Value);
                    m_kes[kljuc] = vrednost;

                    Console.WriteLine("Iz kesa izbacen: " + lru.Value);
                }
            }
        }
    }
}
