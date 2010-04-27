﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentCassandra.Types;
using Apache.Cassandra;

namespace FluentCassandra
{
	class CassandraSuperColumnFamily<T, ST> : CassandraColumnFamily<ST>
		where T : CassandraType
		where ST : CassandraType
	{
		/*
		 * i32 get_count(keyspace, key, column_parent, consistency_level) 
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columnParent"></param>
		/// <returns></returns>
		public override int CountColumns(FluentColumnParent columnParent)
		{
			return CountColumns(
				columnParent.ColumnFamily.Key,
				columnParent.SuperColumn == null ? null : columnParent.SuperColumn.GetNameBytes()
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <returns></returns>
		public int CountColumns(string key, T superColumnName)
		{
			var parent = new ColumnParent {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				parent.Super_column = superColumnName;

			return GetClient().get_count(
				_keyspace.KeyspaceName,
				key,
				parent,
				ConsistencyLevel.ONE
			);
		}

		/*
		 * insert(keyspace, key, column_path, value, timestamp, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		public void InsertColumn(IFluentColumn col)
		{
			InsertColumn(col.GetPath());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public override void InsertColumn(FluentColumnPath path)
		{
			if (IsPartOfFamily(path.ColumnFamily))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			InsertColumn(
				path.ColumnFamily.Key,
				path.SuperColumn == null ? null : path.SuperColumn.GetNameBytes(),
				path.Column
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="col"></param>
		protected void InsertColumn(string key, T superColumnName, T columnName, byte[] value, DateTimeOffset timestamp)
		{
			var path = new ColumnPath {
				Column_family = FamilyName,
				Column = columnName
			};

			if (superColumnName != null)
				path.Super_column = superColumnName;

			GetClient().insert(
				_keyspace.KeyspaceName,
				key,
				path,
				value,
				timestamp.UtcTicks,
				ConsistencyLevel.ONE
			);
		}

		/*
		 * remove(keyspace, key, column_path, timestamp, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		public void Remove(IFluentColumn col)
		{
			Remove(col.GetPath());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void Remove(FluentColumnPath path)
		{
			Remove(
				path.ColumnFamily,
				path.SuperColumn == null ? null : path.SuperColumn.GetNameBytes(),
				path.Column == null ? null : path.Column.GetNameBytes()
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="record"></param>
		/// <param name="columnName"></param>
		/// <param name="superColumnName"></param>
		public void Remove(IFluentColumnFamily record, T superColumnName = null, T columnName = null)
		{
			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			Remove(record.Key, superColumnName, columnName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columnFamily"></param>
		/// <param name="key"></param>
		/// <param name="columnName"></param>
		public void Remove(string key, byte[] superColumnName, T columnName)
		{
			var path = new ColumnPath {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				path.Super_column = superColumnName;

			if (columnName != null)
				path.Column = columnName;

			GetClient().remove(
				_keyspace.KeyspaceName,
				key,
				path,
				DateTimeOffset.UtcNow.Ticks,
				ConsistencyLevel.ONE
			);
		}

		/*
		 * ColumnOrSuperColumn get(keyspace, key, column_path, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		public IFluentColumn GetColumn(IFluentColumn col)
		{
			return GetColumn(col.GetPath());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public IFluentColumn GetColumn(FluentColumnPath path)
		{
			return GetColumn(
				path.ColumnFamily,
				path.SuperColumn == null ? null : path.SuperColumn.GetNameBytes(),
				path.Column == null ? null : path.Column.GetValueBytes()
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="record"></param>
		/// <param name="columnName"></param>
		/// <param name="superColumnName"></param>
		public IFluentColumn GetColumn(IFluentColumnFamily record, T superColumnName = null, T columnName = null)
		{
			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return GetColumn(record.Key, superColumnName, columnName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public IFluentColumn GetColumn(string key, T superColumnName, T columnName)
		{
			var path = new ColumnPath {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				path.Super_column = superColumnName;

			if (columnName != null)
				path.Column = columnName;

			var output = GetClient().get(
				_keyspace.KeyspaceName,
				key,
				path,
				ConsistencyLevel.ONE
			);

			return ObjectHelper.ConvertToFluentColumn(output);
		}

		/*
		 * list<ColumnOrSuperColumn> get_slice(keyspace, key, column_parent, predicate, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnNames"></param>
		/// <returns></returns>
		public IFluentColumnFamily GetSingle(string key, List<T> columnNames)
		{
			return GetSingle(
				key,
				(byte[])null,
				columnNames
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public IFluentColumnFamily GetSingle(string key, FluentColumnParent parent, List<T> columnNames)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return GetSingle(
				key,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				columnNames
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public IFluentColumnFamily GetSingle(string key, T superColumnName, List<T> columnNames)
		{
			var predicate = ObjectHelper.CreateSlicePredicate(columnNames);
			return GetSingle(key, superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public IFluentColumnFamily GetSingle(string key, FluentColumnParent parent, T start, T finish, bool reversed = false, int count = 100)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return GetSingle(
				key,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				start,
				finish,
				reversed,
				count
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public IFluentColumnFamily GetSingle(string key, T superColumnName, T start, T finish, bool reversed = false, int count = 100)
		{
			var predicate = ObjectHelper.CreateSlicePredicate(start, finish, reversed, count);
			return GetSingle(key, superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		protected IFluentColumnFamily GetSingle(string key, byte[] superColumnName, SlicePredicate predicate)
		{
			var parent = new ColumnParent {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				parent.Super_column = superColumnName;

			var output = GetClient().get_slice(
				_keyspace.KeyspaceName,
				key,
				parent,
				predicate,
				ConsistencyLevel.ONE
			);

			var record = ObjectHelper.ConvertToFluentColumnFamily(key, FamilyName, superColumnName, output);
			_context.Attach(record);
			record.MutationTracker.Clear();
			return record;
		}

		/*
		 * map<string,list<ColumnOrSuperColumn>> multiget_slice(keyspace, keys, column_parent, predicate, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public IEnumerable<IFluentColumnFamily> Get(IEnumerable<string> keys, FluentColumnParent parent, List<T> columnNames)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return Get(
				keys,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				columnNames
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> Get(IEnumerable<string> keys, byte[] superColumnName, List<T> columnNames)
		{
			var keysList = keys is List<string> ? (List<string>)keys : keys.ToList();

			var predicate = ObjectHelper.CreateSlicePredicate(columnNames);
			return Get(keysList, superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public IEnumerable<IFluentColumnFamily> Get(IEnumerable<string> keys, FluentColumnParent parent, T start, T finish, bool reversed = false, int count = 100)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return Get(
				keys,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				start,
				finish,
				reversed,
				count
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="columnFamily"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> Get(IEnumerable<string> keys, T superColumnName, T start, T finish, bool reversed = false, int count = 100)
		{
			var keysList = keys is List<string> ? (List<string>)keys : keys.ToList();

			var predicate = ObjectHelper.CreateSlicePredicate(start, finish, reversed, count);
			return Get(keysList, superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="superColumnName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		protected IEnumerable<IFluentColumnFamily> Get(List<string> keys, T superColumnName, SlicePredicate predicate)
		{
			var parent = new ColumnParent {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				parent.Super_column = superColumnName;

			var output = GetClient().multiget_slice(
				_keyspace.KeyspaceName,
				keys,
				parent,
				predicate,
				ConsistencyLevel.ONE
			);

			foreach (var record in output)
			{
				var family = ObjectHelper.ConvertToFluentColumnFamily(record.Key, FamilyName, superColumnName, record.Value);
				_context.Attach(family);
				family.MutationTracker.Clear();
				yield return family;
			}
		}

		/*
		 * list<KeySlice> get_range_slices(keyspace, column_parent, predicate, range, consistency_level)
		 */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyRange"></param>
		/// <param name="parent"></param>
		/// <param name="columnNames"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> GetRange(CassandraKeyRange keyRange, FluentColumnParent parent, List<T> columnNames)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return GetRange(
				keyRange,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				columnNames
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyRange"></param>
		/// <param name="superColumnName"></param>
		/// <param name="columnNames"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> GetRange(CassandraKeyRange keyRange, T superColumnName, List<T> columnNames)
		{
			var predicate = ObjectHelper.CreateSlicePredicate(columnNames);
			return GetRange(ObjectHelper.CreateKeyRange(keyRange), superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyRange"></param>
		/// <param name="parent"></param>
		/// <param name="start"></param>
		/// <param name="finish"></param>
		/// <param name="reversed"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> GetRange(CassandraKeyRange keyRange, FluentColumnParent parent, T start, T finish, bool reversed = false, int count = 100)
		{
			var record = parent.ColumnFamily;

			if (IsPartOfFamily(record))
				throw new FluentCassandraException("The record passed in is not part of this family.");

			return GetRange(
				keyRange,
				parent.SuperColumn == null ? null : parent.SuperColumn.GetNameBytes(),
				start,
				finish,
				reversed,
				count
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyRange"></param>
		/// <param name="superColumnName"></param>
		/// <param name="start"></param>
		/// <param name="finish"></param>
		/// <param name="reversed"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public IEnumerable<IFluentColumnFamily> GetRange(CassandraKeyRange keyRange, T superColumnName, T start, T finish, bool reversed = false, int count = 100)
		{
			var predicate = ObjectHelper.CreateSlicePredicate(start, finish, reversed, count);
			return GetRange(ObjectHelper.CreateKeyRange(keyRange), superColumnName, predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyRange"></param>
		/// <param name="superColumnName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		protected IEnumerable<IFluentColumnFamily> GetRange(KeyRange keyRange, T superColumnName, SlicePredicate predicate)
		{
			var parent = new ColumnParent {
				Column_family = FamilyName
			};

			if (superColumnName != null)
				parent.Super_column = superColumnName;

			var output = GetClient().get_range_slices(
				_keyspace.KeyspaceName,
				parent,
				predicate,
				keyRange,
				ConsistencyLevel.ONE
			);

			foreach (var record in output)
			{
				var family = ObjectHelper.ConvertToFluentColumnFamily(record.Key, FamilyName, superColumnName, record.Columns);
				_context.Attach(family);
				family.MutationTracker.Clear();
				yield return family;
			}
		}
	}
}