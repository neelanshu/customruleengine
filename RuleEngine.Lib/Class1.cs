using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuleEngine.Lib;

namespace RuleEngine.Lib
{
    public interface IRule<T> where T : IModel
    {
        T Apply(T model);
        IEnumerable<T> ApplyToList(IEnumerable <T> modelList);
    }
}

public class CustomerQuantityRule : IRule <TicketModel > {
        public TicketModel Apply(TicketModel model)
        {
            try {
                return ApplyThisRuleToTicketModel(model);
            }
            catch ( Exception ex)
            {
                throw new RuleApplciationFailureException("CustomerQuantityRule" , ex);
            }
        }

        public IEnumerable<TicketModel > ApplyToList(IEnumerable< TicketModel> modelList) {
            try {
                var applyToList = modelList as IList< TicketModel> ?? modelList.ToList();

                foreach ( var ticketModel in applyToList) {
                    ApplyThisRuleToTicketModel(ticketModel);
                }

                return applyToList;
            }
            catch ( Exception ex) {
                throw new RuleApplciationFailureException("CustomerQuantityRule" , ex);
            }
        }

        private TicketModel ApplyThisRuleToTicketModel(TicketModel model)
        {
            if (model.DisplayQuantity < model.Quantity)
            {
                model.CustomerQuantity = model.DisplayQuantity;
                return model;
            }

            model.CustomerQuantity = model.Quantity;
            return model;
        }
    }
}

 public interface IRuleFactory {
        IList< Func<T, IModel>> GetRulesForModel<T>( IModel model) where T : IModel;
        IEnumerable<Func <IEnumerable <T>, IEnumerable< IModel>>> GetRulesForModelList<T>(IEnumerable <T> modelList) where T : IModel;
    }

 public class GmiRuleFactory : IRuleFactory {
        public IList< Func<T, IModel>> GetRulesForModel<T>( IModel model) where T : IModel {

            var ruleAppliers = new List< Func<T, IModel>>();
                     
            if ( typeof (T) == ( typeof ( EventModel))) {
                ruleAppliers.Add(arg=> new AssignSellOrBuyLinkRule().Apply(model as EventModel ));
                           ruleAppliers.Add(arg => new SetSellEnabledFlagRule().Apply(model as EventModel ));
               return ruleAppliers;
            }

                      if ( typeof(T) == ( typeof( PerformerModel)))
                     {
                           ruleAppliers.Add(arg => new PerformerUrlRule().Apply(model as PerformerModel ));
                            return ruleAppliers;
                     }

                      if ( typeof(T) == ( typeof( CategoryModel)))
                     {
                           ruleAppliers.Add(arg => new BreadCrumbCategoryUrlRule().Apply(model as CategoryModel ));
                            return ruleAppliers;
                     }
                     
                      if ( typeof(T) == ( typeof( TicketModel)))
                      {
                           ruleAppliers.Add(arg => new CustomerQuantityRule().Apply(model as TicketModel ));
                           ruleAppliers.Add(arg => new SplitLogicRule().Apply(model as TicketModel ));
                            return ruleAppliers;
                      }



            return ruleAppliers;
        }

        public IEnumerable<Func <IEnumerable <T>, IEnumerable< IModel>>> GetRulesForModelList<T>(IEnumerable <T> modelList) where T : IModel {

            var ruleAppliers = new List< Func< IEnumerable<T>, IEnumerable<IModel >>>();

            if ( typeof (T) == ( typeof ( TicketModel)))
            {
                ruleAppliers.Add(arg => new CalculateTotalSellPriceRule ().ApplyToList(modelList as List<TicketModel>));
                ruleAppliers.Add(arg => new CustomerQuantityRule().ApplyToList(modelList as List <TicketModel >));
                ruleAppliers.Add(arg => new SplitLogicRule().ApplyToList(modelList as List <TicketModel >));
                           ruleAppliers.Add(arg => new RemoveIfFutureOnSaleDateRule ().ApplyToList(modelList as List<TicketModel>));
            }

var allRuleAppliersForEventModel = gmiRuleFactory.GetRulesForModel<EventModel >(eventModel);

                      foreach ( var ruleApplier in allRuleAppliersForEventModel)
                     {
                           ruleApplier.Invoke(eventModel);
                     }
}
