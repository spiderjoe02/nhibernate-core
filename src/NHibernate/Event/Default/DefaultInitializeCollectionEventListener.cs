using System;
using log4net;
using NHibernate.Cache;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister.Collection;

namespace NHibernate.Event.Default
{
	[Serializable]
	public class DefaultInitializeCollectionEventListener : IInitializeCollectionEventListener
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultInitializeCollectionEventListener));

		/// <summary> called by a collection that wants to initialize itself</summary>
		public void OnInitializeCollection(InitializeCollectionEvent @event)
		{
			IPersistentCollection collection = @event.Collection;
			ISessionImplementor source = @event.Session;

			CollectionEntry ce = source.GetCollectionEntry(collection);
			if (ce == null)
				throw new HibernateException("collection was evicted");
			if (!collection.WasInitialized)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("initializing collection " + MessageHelper.InfoString(ce.LoadedPersister, ce.LoadedKey, source.Factory));
				}

				log.Debug("checking second-level cache");
				bool foundInCache = InitializeCollectionFromCache(ce.LoadedKey, ce.LoadedPersister, collection, source);

				if (foundInCache)
				{
					log.Debug("collection initialized from cache");
				}
				else
				{
					log.Debug("collection not cached");
					ce.LoadedPersister.Initialize(ce.LoadedKey, source);
					log.Debug("collection initialized");

					// TODO: H3.2 not ported
					//if (source.Factory.Statistics.StatisticsEnabled)
					//{
					//  source.Factory.StatisticsImplementor.fetchCollection(ce.getLoadedPersister().Role);
					//}
				}
			}
		}

		/// <summary> Try to initialize a collection from the cache</summary>
		private bool InitializeCollectionFromCache(object id, ICollectionPersister persister, IPersistentCollection collection, ISessionImplementor source)
		{

			if (!(source.EnabledFilters.Count == 0) && persister.IsAffectedByEnabledFilters(source))
			{
				log.Debug("disregarding cached version (if any) of collection due to enabled filters ");
				return false;
			}

			// TODO H3.2 Different behaviour
			//bool useCache = persister.HasCache && source.CacheMode.GetEnabled;
			bool useCache = persister.HasCache;

			if (!useCache)
			{
				return false;
			}
			else
			{
				CacheKey ck = new CacheKey(id, persister.KeyType, persister.Role, source.Factory);
				object ce = persister.Cache.Get(ck, source.Timestamp);

				// TODO: H3.2 not ported
				//if (factory.Statistics.StatisticsEnabled)
				//{
				//  if (ce == null)
				//  {
				//    factory.StatisticsImplementor.secondLevelCacheMiss(persister.Cache.RegionName);
				//  }
				//  else
				//  {
				//    factory.StatisticsImplementor.secondLevelCacheHit(persister.Cache.RegionName);
				//  }
				//}

				if (ce == null)
				{
					return false;
				}
				else
				{
					collection.InitializeFromCache(persister, ce, source.GetCollectionOwner(id,persister));
					// collection.AfterInitialize(persister); TODO present in NH not in H3.2 (NH-739)
					source.GetCollectionEntry(collection).PostInitialize(collection);
					return true;
				}
			}
		}
	}
}