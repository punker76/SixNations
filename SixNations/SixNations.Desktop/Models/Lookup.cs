﻿using System;
using System.Collections.Generic;
using SixNations.Desktop.Interfaces;

namespace SixNations.Desktop.Models
{
    public class Lookup : Dictionary<int, string>, IHttpDataServiceModel
    {
        public Lookup(string lookupName)
        {
            Name = lookupName;
        }

        public Lookup(string lookupName, DataTransferObject[] data) : this(lookupName)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Expected to recieve initialised data.");
            }
            foreach (var dto in data)
            {
                Initialise(dto);
            }
        }

        public Lookup(string lookupName, ResponseRootObject response) 
            : this(lookupName, response?.Data) { }

        public Lookup(string lookupName, IDictionary<int, string> dict) : this(lookupName)
        {
            foreach (var kvp in dict)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        public Lookup(Enum @enum)
            : this(@enum.ToString())
        {
            foreach (var field in Enum.GetValues(@enum.GetType()))
            {
                var value = (int)field;
                if (value > 0)
                {
                    Add(value, field.ToString());
                }
            }
        }

        public void Initialise(DataTransferObject dto)
        {
            if (dto.ContainsKey($"{Name}ID") || dto.ContainsKey("ID"))
            {
                var key = dto.ContainsKey($"{Name}ID") ?
                    (int)dto[$"{Name}ID"] : (int)dto["ID"];
                var value = dto.ContainsKey($"{Name}Name") ?
                    dto[$"{Name}Name"].ToString() : dto["Name"].ToString();
                Add(key, value);
            }
        }

        public string Name { get; }

        public string Error { get; set; }

        public bool IsLockedForEditing => false;

        public bool IsDirty => false;

        public int Id => throw new NotSupportedException();

        public bool IsNew => false;

        public bool IsReadOnly => true;

        public IDictionary<string, object> GetData() => throw new NotSupportedException();
    }
}