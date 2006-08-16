using Shunde;
using Shunde.Framework;
using System;



namespace Peak
{

	/// <summary>A payment made on an <see cref="Order" /></summary>
	public class OrderPayment : OODBObject, Nameable
	{

		/// <summary>The Order that this payment is for</summary>
		private Order order;

		public Order Order
		{
			get { return order; }
			set { order = value; }
		}

		/// <summary>The method of payment</summary>
		private PaymentMethod paymentMethod;

		public PaymentMethod PaymentMethod
		{
			get { return paymentMethod; }
			set { paymentMethod = value; }
		}

		/// <summary>The date that this payment was made</summary>
		private DateTime paymentDate;

		public DateTime PaymentDate
		{
			get { return paymentDate; }
			set { paymentDate = value; }
		}

		/// <summary>The name of the user that entered the payment</summary>
		private Anybody enteredBy;

		public Anybody EnteredBy
		{
			get { return enteredBy; }
			set { enteredBy = value; }
		}

		/// <summary>Notes about this payment</summary>
		private string notes;

		public string Notes
		{
			get { return notes; }
			set { notes = value; }
		}

		/// <summary>Gets the net amount received, that is the amount less taxes and payment method surcharges</summary>
		/// <remarks>A negative number represents a refund</remarks>
		private double netAmountPaid;

		public double NetAmountPaid
		{
			get { return netAmountPaid; }
			set { netAmountPaid = value; }
		}

		/// <summary>The amount of money paid that goes towards the <see cref="paymentMethod" />'s surcharge</summary>
		/// <remarks>A negative number represents a refund</remarks>
		private double surchargePaid;

		public double SurchargePaid
		{
			get { return surchargePaid; }
			set { surchargePaid = value; }
		}

		/// <summary>The amount of money paid that goes towards taxes, eg. Goods and Services Tax (GST)</summary>
		/// <remarks>A negative number represents a refund</remarks>
		private double taxPaid;

		public double TaxPaid
		{
			get { return taxPaid; }
			set { taxPaid = value; }
		}

		/// <summary>Gets the actual amount paid, including surcharges and taxes</summary>
		/// <remarks>This is calculated as netAmountPaid + surchargePaid + taxPaid</remarks>
		public double getActualAmountPaid()
		{
			return NetAmountPaid + SurchargePaid + TaxPaid;
		}

		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		/// <remarks>Only those payments made this millenium will be accepted</remarks>
		static OrderPayment()
		{

			DBTable tbl = new DBTable("OrderPayment", new DBColumn[] {
				new DBColumn( "order", typeof(Order), false ),
				new DBColumn( "paymentMethod", typeof(PaymentMethod), false ),
				new DBColumn( "paymentDate", typeof(DateTime), false, new DateTime(2000, 1, 1), null ),
				new DBColumn( "enteredBy", typeof(String), 1, 200 ),
				new DBColumn( "notes", typeof(String), true ),
				new DBColumn( "netAmountPaid", typeof(double), false),
				new DBColumn( "surchargePaid", typeof(double), false),
				new DBColumn( "taxPaid", typeof(double), false)
			});

			ObjectInfo.registerObjectInfo(typeof(OrderPayment), tbl);

		}


		/// <summary>Sets the amount, surcharge, and tax for this payment</summary>
		/// <remarks>Uses the surcharge percentage as specified in <see cref="paymentMethod" />.</remarks>
		public void setPayment(double amount, double taxRate)
		{
			setPayment(amount, taxRate, PaymentMethod.percentageSurcharge);
		}

		/// <summary>Sets the amount, surcharge, and tax for this payment</summary>
		/// <remarks>Uses the specified surcharge percentage, rather than the one in the <see cref="paymentMethod" />.</remarks>
		public void setPayment(double amount, double taxRate, double surchargePercent)
		{
			double beforeTax = amount / taxRate;

			this.TaxPaid = amount - beforeTax;

			this.NetAmountPaid = beforeTax / (1 + (surchargePercent / 100.0));

			this.SurchargePaid = amount - this.TaxPaid - this.NetAmountPaid;

		}


		/// <summary>Gets the name of this object</summary>
		public String getName
		{
			get
			{
				return PaymentDate.ToString() + ": " + getActualAmountPaid().ToString("C");
			}
		}

		/// <summary>Gets and populates all the OrderPayments for a given <see cref="Order" /></summary>
		/// <remarks>Also populates the <see cref="paymentMethod" /> field.</remarks>
		/// <param name="order">The <see cref="Order" /> that this OrderPayment was made for</param>
		/// <returns>Returns an array of 0 or more OrderPayments</returns>
		public static OrderPayment[] getOrderPayments(Order order)
		{

			Type t = typeof(OrderPayment);

			ObjectInfo oi = ObjectInfo.getObjectInfo(t);

			String sql = "SELECT " + oi.columnClause + " FROM " + oi.fromClause + " WHERE OODBObject.isDeleted = 0 AND OrderPayment.orderId = " + order.id + " ORDER BY OODBObject.displayOrder ASC, OrderPayment.paymentDate ASC";

			OrderPayment[] objs = (OrderPayment[])OODBObject.getObjects(sql, t);
			for (int i = 0; i < objs.Length; i++)
			{
				OrderPayment op = objs[i];
				op.Order = order;
				op.PaymentMethod.populate();
			}
			return objs;

		}


	}

}