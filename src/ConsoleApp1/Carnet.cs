using Hopex.Core;
using Hopex.Core.Domain;
using Hopex.Core.Schemas;
using System;

namespace ConsoleApp1
{
    partial class Program
    {
        public class Carnet : DomainModelElement
        {
            public Carnet() { }
            public Carnet(IHopexUnitOfWork context)
                : base(context, context.Schema.GetMetaClassByName("Carnet")!)
            {
            }

            protected Carnet(IHopexUnitOfWork context, string id, MetaClass metaClass)
                : base(context, metaClass, id, null)
            {
            }

            //private IDomainModelCollection<Contact>? _contacts;
            //public IDomainModelCollection<Contact> Contacts
            //{
            //    get
            //    {
            //        if (_contacts == null)
            //        {
            //            _contacts = GetReference<Contact>(MetaClass.GetMetaRelationshipByPropertyName("Contacts")!);
            //        }
            //        return _contacts;
            //    }
            //}

            public string Name
            {
                get => GetPropertyValue<string>("Name");
                set => SetPropertyValue("Name", value);
            }
            public int Value
            {
                get => GetPropertyValue<int>("Value");
                set => SetPropertyValue("Value", value);
            }
            public DateTime Date
            {
                get => GetPropertyValue<DateTime>("Date");
                set => SetPropertyValue("Date", value);
            }
        }
    }
}
