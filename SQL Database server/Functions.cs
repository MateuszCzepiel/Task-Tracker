using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

namespace Database_Server
{
    internal class Functions
    {
        public static string Hash(string password)
        {
            BigInteger currentHashValue = 0;
            LinkedList<int> currentLinkedList = new LinkedList<int>();
            foreach (char letter in password)
            {
                currentLinkedList.AddLastNode((int)letter);
            }
            BigInteger magicNumber = BigInteger.Parse(password.Length.ToString()) * BigInteger.Parse(((int)password[0]).ToString()) * BigInteger.Parse(((int)password[1]).ToString()) * BigInteger.Parse(((int)password[2]).ToString()) * BigInteger.Parse("42");
            Node<int> currentNode = currentLinkedList.firstNode;
            if (currentNode.nextNode == null)
            {
                currentHashValue = BigInteger.Parse(currentLinkedList.firstNode.value.ToString()) * magicNumber;
            }
            while (currentNode.nextNode != null)
            {
                if (currentHashValue == 0)
                {
                    if (currentNode.nextNode == null)
                    {
                        currentHashValue = BigInteger.Parse(currentNode.value.ToString()) * magicNumber;
                        Console.WriteLine("NOT USEDDD");
                    }
                    else
                    {
                        currentHashValue = (BigInteger.Parse(currentNode.value.ToString()) * magicNumber) + BigInteger.Parse(currentNode.nextNode.value.ToString());
                    }
                }
                else
                {
                    if (currentNode.nextNode == null)
                    {
                        currentHashValue = currentHashValue * magicNumber;
                    }
                    else
                    {
                        currentHashValue = (currentHashValue * magicNumber) + BigInteger.Parse(currentNode.nextNode.value.ToString());
                    }
                }
                currentNode = currentNode.nextNode;
            }
            return currentHashValue.ToString("X");
        }
    }
}
