using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace _8_Puzzle_Problem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Generate game board.
        Button[,] gameBtn = new Button[3,3];
        private void Form1_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameBtn[j, i] = new Button();
                    gameBtn[j, i].Size = new Size(100,100);
                    gameBtn[j, i].Location = new Point(15 + j * 100, 50 + i * 100);
                    gameBtn[j, i].Text = (j + i * 3).ToString();
                    gameBtn[j, i].Font = new Font("新細明體",20);
                    Controls.Add(gameBtn[j, i]);
                }
            }
        }
        Random rnd = new Random();
        int index = 0,cnt=0;
        byte[] source = new byte[9];//Source status.
        byte[] goal = new byte[9] {1,2,3,4,5,6,7,8,0};//Goal status.
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            label3.Text = "Search Time : ??:??:??.??????? s";
            label1.Text = "Need : ??/?? steps";
            count = 0;
            restart:
            for (int i = 0; i < 9; i++)
            {
                source[i] = (byte)rnd.Next(9);
                for (int j = 0; j < i; j++)
                {
                    if (source[i] == source[j])
                        i--;
                }
            }
            cnt = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = i + 1; j < 9; j++)
                {
                    if (source[i] > source[j] && source[j] != 0)
                        cnt++;
                }
            }
            //Determine if the game can be solved.
            if (cnt % 2==1)
            {
                goto restart;
            }
            draw(source);
        }

        Stopwatch sw = new Stopwatch();//Count the search time.
        List<Node> path = new List<Node>();//Record the shortest path.
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Text = "Searching...";
            button2.Enabled = false;
            sw = Stopwatch.StartNew();
            path = Solve(source, goal);
            sw.Stop();
            button2.Text = "Start Search";
            button2.Enabled = true;
            label3.Text = "Search Time : " + sw.Elapsed + " s";
            label1.Text = "Need : " + "0/" + path.Count + " steps";
            timer1.Start();
        }
        int count = 0;
        //Replay the auto path.
        private void button3_Click(object sender, EventArgs e)
        {
            count = 0;
            button3.Enabled = false;
            timer1.Start();
        }
        //Play auto path.
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (count < path.Count)
            {
                draw(path[count++].status);
                label1.Text = "Need : " + count + "/" + path.Count + " steps";
            }
            else
            {
                timer1.Stop();
                button3.Enabled = true;
            }
        }
        //Draw the current status.
        private void draw(byte[] status)
        {
            index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameBtn[j, i].Text = status[index++].ToString();
                }
            }
        }
        
        public class Node
        {
            public byte[] status; // Now status
            public Node father; // Record for previous status.If father=null,then this is root node.
            public Node(byte[] status, Node father)
            {
                this.status = status;
                this.father = father;
            }

            //Byte array to int.Ex:{1,2,3,4,5,6,7,8,0}=>123456780
            public int ToSequence()
            {
                int result = 0;
                for (int i = 0; i < status.Length; i++)
                    result = result * 10 + status[i];
                return result;
            }
        }
        
        List<Node> GetNext(Node now)// Get next node.
        {
            int index = Array.IndexOf<byte>(now.status, 0);
            int col = index % 3;
            int row = index / 3;

            List<Node> nextPush = new List<Node>();
            byte[] next;

            if (row != 0) // Top
            {
                next = (byte[])now.status.Clone();
                swap(ref next[index], ref next[index - 3]);// swap with top one.
                nextPush.Add(new Node(next, now));// Add this status.
            }

            if (col != 2) // Right
            {
                next = (byte[])now.status.Clone();
                swap(ref next[index], ref next[index + 1]);// swap with right one.
                nextPush.Add(new Node(next, now));
            }

            if (row != 2) // Bottom
            {
                next = (byte[])now.status.Clone();
                swap(ref next[index], ref next[index + 3]);// swap with bottom one.
                nextPush.Add(new Node(next, now));
            }

            if (col != 0) // Left
            {
                next = (byte[])now.status.Clone();
                swap(ref next[index], ref next[index - 1]);// swap with left one.
                nextPush.Add(new Node(next, now));
            }
            return nextPush;
        }
        //swap two block.
        static void swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        // Input source status and goal status;Output the shortest path count.
        List<Node> Solve(byte[] source, byte[] goal)
        {
            Queue<Node> queue = new Queue<Node>();

            // Use a sequence of states to store paths that have been taken, preventing them from going back.
            SortedList<int, bool> book = new SortedList<int, bool>();

            Node end = new Node(goal, null);// END
            Node start = new Node(source, null);// START

            queue.Enqueue(start);// Push START
            book.Add(start.ToSequence(), true);// Mark the START that has passed,to prevent going back.

            int endStatus = end.ToSequence();
            while (queue.Count > 0)
            {
                // Get current search status,and pull it.
                Node now = queue.Dequeue();
                // If the end point is reached, then the output path.
                if (now.ToSequence() == endStatus)
                    return PathTrace(now);
                
                // Get the location that can be walked.
                List<Node> nextPath = GetNext(now);
                foreach (var path in nextPath)
                {
                    int sign = path.ToSequence();

                    // Determine if the current node status has been expanded
                    if (!book.Keys.Contains(sign))
                    {
                        // Push the current status into queue and mark the path has passed.                
                        queue.Enqueue(path);
                        book.Add(sign, true);
                    }
                }
            }

            // If the path is not found, the representative has no solution.
            return null;
        }
        
        List<Node> PathTrace(Node now)
        {
            // Trace the path.
            List<Node> path = new List<Node>();
            while (now.father != null)
            {
                path.Add(now);
                now = now.father;
            }
            path.Reverse();
            return path;
        }
        
    }
}
