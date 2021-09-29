using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructure
{

    public interface ITree<T>
    {
        T Root();                                //求树的根结点
        T Parent(T t);                           //求结点t的双亲结点
        T Child(T t, int i);                     //求结点t的第i个子结点
        T RightSibling(T t);                     //求结点t第一个右边兄弟结点
        bool Insert(T s, T t, int i);            //将树s加入树中作为结点t的第i颗子树
        T Delete(T t, int i);                    //删除结点t的第i颗子树
        void Traverse(int TraverseType);         //按某种方式遍历树
        void Clear();                            //清空树
        bool IsEmpty();                          //判断是否为空
        int GetDepth(T t);                          //求树的深度
    }


    /// <summary>
    /// 循环顺序队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class CSeqQueue<T>
    {
        private int maxsize;       //循环顺序队列的容量
        private T[] data;          //数组，用于存储循环顺序队列中的数据元素
        private int front;         //指示最近一个已经离开队列的元素所占有的位置 循环顺序队列的对头
        private int rear;          //指示最近一个进入队列的元素的位置           循环顺序队列的队尾

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        //容量属性
        public int Maxsize
        {
            get { return maxsize; }
            set { maxsize = value; }
        }

        //对头指示器属性
        public int Front
        {
            get { return front; }
            set { front = value; }
        }

        //队尾指示器属性
        public int Rear
        {
            get { return rear; }
            set { rear = value; }
        }

        public CSeqQueue()
        {

        }

        public CSeqQueue(int size)
        {
            data = new T[size];
            maxsize = size;
            front = rear = -1;
        }

        //判断循环顺序队列是否为满
        public bool IsFull()
        {
            if ((front == -1 && rear == maxsize - 1) || (rear + 1) % maxsize == front)
                return true;
            else
                return false;
        }

        //清空循环顺序列表
        public void Clear()
        {
            front = rear = -1;
        }

        //判断循环顺序队列是否为空
        public bool IsEmpty()
        {
            if (front == rear)
                return true;
            else
                return false;
        }

        //入队操作
        public void EnQueue(T elem)
        {
            if (IsFull())
            {
                Console.WriteLine("Queue is Full !");
                return;
            }
            rear = (rear + 1) % maxsize;
            data[rear] = elem;
        }

        //出队操作
        public T DeQueue()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Queue is Empty !");
                return default(T);
            }
            front = (front + 1) % maxsize;
            return data[front];
        }

        //获取对头数据元素
        public T GetFront()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Queue is Empty !");
                return default(T);
            }
            return data[(front + 1) % maxsize];//front从-1开始
        }

        //求循环顺序队列的长度
        public int GetLength()
        {
            return (rear - front + maxsize) % maxsize;
        }
    }

    /// <summary>
    /// 树的多链表结点类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class WT_Node<T>
    {
        private T data;                   //存储结点的数据
        private WT_Node<T>[] childs;       //存储子结点的位置

        public WT_Node(int max)
        {
            childs = new WT_Node<T>[max];
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i] = null;
            }
        }

        public T Data
        {
            get { return data; }
            set { data = value; }
        }

        public WT_Node<T>[] Childs
        {
            get { return childs; }
            set { childs = value; }
        }
    }



    class WT_Tree<T> : ITree<WT_Node<T>>
    {
        private WT_Node<T> head;

        public WT_Node<T> Head
        {
            get { return head; }
            set { head = value; }
        }

        public WT_Tree()
        {
            head = null;
        }

        public WT_Tree(WT_Node<T> node)
        {
            head = node;
        }

        //求树的根结点
        public WT_Node<T> Root()
        {
            return head;
        }

        public void Clear()
        {
            head = null;
        }

        //待测试！！！
        public int GetDepth(WT_Node<T> root)
        {
            int len;
            if (root == null)
            {
                return 0;
            }
            for (int i = 0; i < root.Childs.Length; i++)
            {
                if (root.Childs[i] != null)
                {
                    len = GetDepth(root.Childs[i]);
                    return len + 1;
                }
            }
            return 0;
        }

        public bool IsEmpty()
        {
            return head == null;
        }

        //求结点t的双亲结点，如果t的双亲结点存在，返回双亲结点，否则返回空
        //按层序遍历的算法进行查找
        public WT_Node<T> Parent(WT_Node<T> t)
        {
            WT_Node<T> temp = head;
            if (IsEmpty() || t == null) return null;
            if (temp.Data.Equals(t.Data)) return null;
            CSeqQueue<WT_Node<T>> queue = new CSeqQueue<WT_Node<T>>(50);
            queue.EnQueue(temp);
            while (!queue.IsEmpty())
            {
                temp = (WT_Node<T>)queue.DeQueue();
                for (int i = 0; i < temp.Childs.Length; i++)
                {
                    if (temp.Childs[i] != null)
                    {
                        if (temp.Childs[i].Data.Equals(t.Data))
                        {
                            return temp;
                        }
                        else
                        {
                            queue.EnQueue(temp.Childs[i]);
                        }
                    }
                }
            }
            return null;
        }

        //求结点t的第i个子结点。如果存在，返回第i个子结点，否则返回空
        //i=0时，表示求第一个子节点
        public WT_Node<T> Child(WT_Node<T> t, int i)
        {
            if (t != null && i < t.Childs.Length)
            {
                return t.Childs[i];
            }
            else
            {
                return null;
            }
        }
        //查找nc节点在np子节点中的位置，若nc、np任意为空或nc不是np子节点则返回-1
        public int FindChildRank(WT_Node<T> np, WT_Node<T> nc)
        {
            if (np!=null)
            {
                var dic = GetLinkedChildren(np);
                if (dic==null)
                {
                    return -1;
                }
                var tt= dic.FirstOrDefault(_ => _.Value.Data.Equals(nc.Data));
                return tt.Value == null ? -1 : tt.Key;
            }
            return -1;
        }

        //求结点t第一个右边兄弟结点，如果存在，返回第一个右边兄弟结点，否则返回空
        public WT_Node<T> RightSibling(WT_Node<T> t)
        {
            WT_Node<T> pt = Parent(t);
            if (pt != null)
            {
                int i = FindRank(t);
                return Child(pt, i + 1);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 以循环方式查找当前节点的偏移offset的兄弟节点（offset为负表示左兄弟）
        /// </summary>
        /// <param name="t"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public WT_Node<T> FindSibling(WT_Node<T> t,int offset)
        {
            WT_Node<T> pt = Parent(t);
            if (pt != null)
            {
                int j = 0;
                var dic= pt.Childs.Where(_ => _ != null).ToDictionary(p=> j++,p=>p) ;
                var v= dic.FirstOrDefault(_ => _.Value.Data.Equals(t.Data));
                int i = 0;
                if (v.Value!=null)
                {
                    i = dic.FirstOrDefault(_ => _.Value.Data.Equals(t.Data)).Key;
                    offset %= dic.Count;
                    i += offset;
                    i += dic.Count;
                    i %= dic.Count;
                }
                else
                {
                    return null;
                }
                return dic[i];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<int, WT_Node<T>> GetLinkedChildren(WT_Node<T> t)
        {
            if (t!=null)
            {
                int j = 0;
                var dic = t.Childs.Where(_ => _ != null).ToDictionary(p => j++, p => p);
                return dic.Count==0?null:dic;
            }
            return null;
        }

        //查找结点t在兄弟中的排行，成功时返回位置，否则返回-1
        private int FindRank(WT_Node<T> t)
        {
            WT_Node<T> pt = Parent(t);
            for (int i = 0; i < pt.Childs.Length; i++)
            {
                WT_Node<T> temp = pt.Childs[i];
                if (temp != null && temp.Data.Equals(t.Data))
                {
                    return i;
                }
            }
            return -1;
        }

        //查找在树中的结点t，成功是返回t的位置，否则返回null
        private WT_Node<T> FindNode(WT_Node<T> t)
        {
            if (head.Data.Equals(t.Data)) return head;
            WT_Node<T> pt = Parent(t);
            foreach (WT_Node<T> temp in pt.Childs)
            {
                if (temp != null && temp.Data.Equals(t.Data))
                {
                    return temp;
                }
            }
            return null;
        }

        //把以s为头结点的树插入到树中作为结点t的第i颗子树。成功返回true
        public bool Insert(WT_Node<T> s, WT_Node<T> t, int i)
        {
            if (IsEmpty()) head = t;
            t = FindNode(t);
            if (t != null && t.Childs.Length > i)
            {
                t.Childs[i] = s;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Insert(WT_Node<T> s, ushort id, int i)
        {
            WT_Node<T> t = new WT_Node<MyData_WT_Data>(10) { Data = new MyData_WT_Data(id) } as WT_Node<T>;
            if (IsEmpty()) head = t as WT_Node<T>;
            t = FindNode(t);
            int j = 0;
            var dic= t.Childs.ToDictionary(p => j++, p => p);
            i=dic.FirstOrDefault((_) => _.Value == null).Key;
            if (t != null && t.Childs.Length > i)
            {
                t.Childs[i] = s;
                return true;
            }
            else
            {
                return false;
            }
        }


        //删除结点t的第i个子树。返回第i颗子树的根结点。
        public WT_Node<T> Delete(WT_Node<T> t, int i)
        {
            t = FindNode(t);
            WT_Node<T> node = null;
            if (t != null && t.Childs.Length > i)
            {
                node = t.Childs[i];
                t.Childs[i] = null;
            }
            return node;
        }


        //先序遍历
        //根结点->遍历根结点的左子树->遍历根结点的右子树 
        public void preorder(WT_Node<T> root)
        {
            if (root == null)
                return;
            Console.WriteLine(root.Data + " ");
            for (int i = 0; i < root.Childs.Length; i++)
            {
                preorder(root.Childs[i]);
            }
        }


        //后序遍历
        //遍历根结点的左子树->遍历根结点的右子树->根结点
        public void postorder(WT_Node<T> root)
        {
            if (root == null)
            { return; }
            for (int i = 0; i < root.Childs.Length; i++)
            {
                postorder(root.Childs[i]);
            }
            Console.WriteLine(root.Data + " ");
        }


        //层次遍历
        //引入队列 
        public void LevelOrder(WT_Node<T> root)
        {
            Console.WriteLine("遍历开始：");
            if (root == null)
            {
                Console.WriteLine("没有结点！");
                return;
            }

            WT_Node<T> temp = root;

            CSeqQueue<WT_Node<T>> queue = new CSeqQueue<WT_Node<T>>(50);
            queue.EnQueue(temp);
            while (!queue.IsEmpty())
            {
                temp = (WT_Node<T>)queue.DeQueue();
                Console.WriteLine(temp.Data + " ");
                for (int i = 0; i < temp.Childs.Length; i++)
                {
                    if (temp.Childs[i] != null)
                    {
                        queue.EnQueue(temp.Childs[i]);
                    }
                }
            }
            Console.WriteLine("遍历结束！");
        }

        //按某种方式遍历树
        //0 先序
        //1 后序
        //2 层序
        public void Traverse(int TraverseType)
        {
            if (TraverseType == 0) preorder(head);
            else if (TraverseType == 1) postorder(head);
            else LevelOrder(head);
        }
    }

}

class MyData_WT_Data
{
    private ushort did;
    private string content;
    private string d_value;
    private string title_content;
    private ushort pid;
    private byte posx;
    private byte posy;
    private byte pageid;
    private byte len;
    private bool selected;
    private byte d_type;

    public MyData_WT_Data(ushort id, string content = "",string title_content="", string dValue = null,ushort pid=0, byte posx = 0, byte posy = 0, byte pageid = 0, byte len = 0, bool selected = false,byte dtype=0)
    {
        this.did = id;
        this.content = content;
        this.title_content = title_content;
        this.DValue = dValue;
        this.pid = pid;
        this.posx = posx;
        this.posy = posy;
        this.pageid = pageid;
        this.len = len;
        this.Selected = selected;
        this.d_type = dtype;
    }

    public MyData_WT_Data() { }

    public ushort DId
    {
        set { did = value; }
        get { return did; }
    }

    public string Content
    {
        set { content = value; }
        get { return content; }
    }

    public string TitleContent
    {
        set { title_content = value; }
        get { return title_content; }
    }

    public string DValue
    {
        get { return d_value; }
        set { d_value = value; }
    }

    public ushort PId//父节点ID
    {
        set { pid = value; }
        get { return pid; }
    }
    public byte Posx
    {
        set { posx = value; }
        get { return posx; }
    }

    public byte Posy
    {
        set { posy = value; }
        get { return posy; }
    }

    public byte Pageid
    {
        set { pageid = value; }
        get { return pageid; }
    }

    public byte Len
    {
        set { len = value; }
        get { return len; }
    }

    public bool Selected
    {
        get { return selected; }
        set { selected = value; }
    }
    /// <summary>
    /// 显示类型：0 可选择，有下一级；1 可选择，无下一级；2 不可选择；3 使用特殊方法（现仅抄表使用）；4 可选择，使用模板
    /// </summary>
    public byte DType
    {
        set { d_type = value; }
        get { return d_type; }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        MyData_WT_Data p = (MyData_WT_Data)obj;
        return p.DId == DId;

    }

    public override string ToString()
    {
        return $"{did.ToString("X4")}\t{content}\t{DValue}\r\n";
    }
}

/*
 *
 *         static void F17()
        {
            WT_Node<MyData> root = new WT_Node<MyData>(10);
            root.Data=new MyData(0x0001,"root",null,0,10,1,4);

            WT_Tree<MyData> tree = new WT_Tree<MyData>(root);

            WT_Node<MyData> c11 = new WT_Node<MyData>(10);
            c11.Data = new MyData(0x0100, "c11", null, 0, 0, 1, 3);
            tree.Insert(c11, id:0x0001, 0);

            WT_Node<MyData> c12 = new WT_Node<MyData>(10);
            c12.Data = new MyData(0x0101, "c12", null, 1, 2, 1, 3);
            tree.Insert(c12, root, 1);

            WT_Node<MyData> c13 = new WT_Node<MyData>(10);
            c13.Data = new MyData(0x0102, "c13", null, 2, 4, 1, 3);
            tree.Insert(c13, root, 2);

            WT_Node<MyData> c21 = new WT_Node<MyData>(10);
            c21.Data = new MyData(0x0201, "c21", null, 0, 0, 1, 3);
            tree.Insert(c21, c11, 0);

            WT_Node<MyData> curNode = null;
            while (true)
            {
                if (curNode==null)//当前节点没有父节点
                {
//                    //显示root
//                    var troot = tree.Head;
                    Console.Write(ShowNode(null, tree));
                }
                else
                {
                    Console.Write(ShowNode(curNode,tree));
                }

                Console.WriteLine("select node:");
                char ch = Convert.ToChar(Console.ReadLine());
                if (ch=='1')
                {
                    if (curNode==null)
                    {
                        curNode = tree.Head;
                    }
                    else
                    {
                        curNode = curNode.Childs[0];
                    }
                }
                Console.Clear();
            }
        }

        static string ShowNode(WT_Node<MyData> node,WT_Tree<MyData> tree)
        {
            string s = "";
            string[] ss=new string[8];
            if (node==null)
            {
                ss[tree.Head.Data.Posx] = tree.Head.Data.Content.PadLeft(tree.Head.Data.Len + tree.Head.Data.Posy);
            }
            else
            {
                foreach (var nodeChild in node.Childs)
                {
                    if (nodeChild==null)
                    {
                        break;
                    }
                    ss[nodeChild.Data.Posx] = nodeChild.Data.Content.PadLeft(nodeChild.Data.Len + nodeChild.Data.Posy);
                }
            }

            foreach (var s1 in ss)
            {
                s += s1 + "\r\n";
            }
            return s;
        }
 */
