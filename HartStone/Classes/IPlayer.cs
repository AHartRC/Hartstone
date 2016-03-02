using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HartStone.Models;

namespace HartStone.Classes
{
    /// <summary>
    /// <para>An interface that isn't needed but was put in to show working knowledge of interfaces,</para>
    /// <para>and multi-line formatted code comments! =-)</para>
    /// </summary>
    public interface IPlayer
    {
        TAG_CLASS CurrentClass { get; set; }
        IEnumerable<HSCard> CurrentDeck { get; set; }
        string Name { get; set; }
    }
}
