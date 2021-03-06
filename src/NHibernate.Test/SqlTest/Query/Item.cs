using Iesi.Collections;

namespace NHibernate.Test.SqlTest.Query
{
	public class Item
	{
		private int id;
		private int alternativeItemId;

		private ISet alternativeItems;

		protected Item()
		{
		}

		public Item(int id, int alternativeItemId)
		{
			this.id = id;
			this.alternativeItemId = alternativeItemId;
		}

		public virtual int Id
		{
			get { return id; }
			set { id = value; }
		}

		public virtual int AlternativeItemId
		{
			get { return alternativeItemId; }
			set { alternativeItemId = value; }
		}

		public virtual ISet AlternativeItems
		{
			get { return alternativeItems; }
			set { alternativeItems = value; }
		}
	}
}