using System;
using Gtk;
using NLog;

namespace earchive
{
	class ImageTreeStore : Gtk.TreeStore, TreeDragSourceImplementor, TreeDragDestImplementor
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ImageTreeStore(params Type[] types) : base (types)
		{
			this.RowDeleted += OnImageListRowRemoved;
		}

		public new bool RowDraggable (TreePath path)
		{
			return path.Depth == 2;
		}

		public new bool RowDropPossible(TreePath path, SelectionData sel)
		{
			return path.Depth == 2;
		}

		public new bool DragDataGet(TreePath path, SelectionData sel)
		{
			logger.Debug("DragDataGet path={0}", path);
			return Tree.SetRowDragData(sel, this, path);
		}

		public new bool DragDataReceived(TreePath path, SelectionData data)
		{
			logger.Debug("DragDataReceived dstPath={0}", path);
			TreeModel srcModel;
			TreePath srcPath;
			TreeIter srcIter, dstIter, newIter, ParentIter;
			if(Tree.GetRowDragData(data, out srcModel, out srcPath))
			{
				logger.Debug("DragDataReceived srcPath={0}", srcPath);
				bool Last = false;
				if(!this.GetIter(out dstIter, path))
				{
					path.Prev();
					Last = true;
					this.GetIter(out dstIter, path);
				}
				this.GetIter(out srcIter, srcPath);
				this.IterParent(out ParentIter, dstIter);
				if(Last)
					newIter = this.InsertNodeAfter(ParentIter, dstIter);
				else
					newIter = this.InsertNodeBefore(ParentIter, dstIter);
				CopyValues(srcIter, newIter);
				return true;
			}
			return false;
		}

		public void CopyValues(TreeIter srcIter, TreeIter dstIter)
		{
			for(int i = 0; i < this.NColumns; i++)
			{
				object value = this.GetValue(srcIter, i);
				if (value == null)
					continue;
				this.SetValue(dstIter, i, value);
			}
		}

		public new bool DragDataDelete(TreePath path)
		{
			TreeIter iter;
			this.GetIter(out iter, path);
			this.Remove(ref iter);
			return true;
		}

		protected void OnImageListRowRemoved(object o, RowDeletedArgs arg)
		{
			TreeIter iter;
			if(arg.Path.Depth == 2)
			{
				arg.Path.Up ();
				this.GetIter (out iter, arg.Path);
				if(!this.IterHasChild (iter))
					this.Remove (ref iter);
			}
		}
	}
}

