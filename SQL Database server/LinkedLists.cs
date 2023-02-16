using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Server
{
    public class LinkedList<T>
    {
        public Node<T> firstNode;
        public Node<T> lastNode;
        public int numberOfNodesInList = 0;

        public void AddFirstNode(T data)
        {
            numberOfNodesInList++;
            Node<T> newNodeToAdd = new Node<T>(data, null);
            if (firstNode == null)
            {
                firstNode = newNodeToAdd;
                lastNode = newNodeToAdd;
                return;
            }
            newNodeToAdd.SetNextNode(firstNode);
            firstNode = newNodeToAdd;
        }

        public void AddLastNode(T data)
        {
            numberOfNodesInList++;
            Node<T> newNodeToAdd = new Node<T>(data, null);
            if (firstNode == null)
            {
                firstNode = newNodeToAdd;
                lastNode = newNodeToAdd;
                return;
            }
            lastNode.SetNextNode(newNodeToAdd);
            lastNode = newNodeToAdd;
        }

        public void AddAfterNode(T afterData, T dataToAdd)
        {
            Node<T> currentNode = firstNode;
            for (int position = 0; position < numberOfNodesInList; position++)
            {
                if (currentNode.GetData().Equals(afterData))
                {
                    Node<T> newNodeToAdd = new Node<T>(dataToAdd, currentNode.GetNextNode());
                    currentNode.SetNextNode(newNodeToAdd);
                    if (currentNode == lastNode)
                    {
                        lastNode = newNodeToAdd;
                    }
                    numberOfNodesInList++;
                    return;
                }
                currentNode = currentNode.GetNextNode();
            }
            Console.WriteLine("Element not found in list. Nothing was added");
        }

        public void Output()
        {
            Node<T> currentNode = firstNode;
            for (int position = 0; position < numberOfNodesInList; position++)
            {
                Console.WriteLine(currentNode.GetData());
                currentNode = currentNode.GetNextNode();
            }
        }

        public bool CheckIfContains(T data)
        {
            Node<T> currentNode = firstNode;
            while (currentNode != null)
            {
                if (currentNode.GetData().Equals(data))
                {
                    return true;
                }
                currentNode = currentNode.GetNextNode();
            }
            return false;
        }

        public int FindData(T data)
        {
            Node<T> currentNode = firstNode;
            for (int position = 0; position < numberOfNodesInList; position++)
            {
                if (currentNode.GetData().Equals(data))
                {
                    return position;
                }
                currentNode = currentNode.GetNextNode();
            }
            return -1;
        }
    }

    public class Node<T>
    {
        public T value;
        public Node<T> nextNode;

        public Node(T value, Node<T> nextNode)
        {
            this.value = value;
            this.nextNode = nextNode;
        }

        public T GetData()
        {
            return value;
        }
        public void SetData(T value)
        {
            this.value = value;
        }
        public Node<T> GetNextNode()
        {
            return nextNode;
        }
        public void SetNextNode(Node<T> node)
        {
            nextNode = node;
        }
    }
}
