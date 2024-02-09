// Copyright (C) 2015-2024 The Neo Project.
//
// OracleRequest.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.IO;
using Neo.VM;
using Neo.VM.Types;
using Array = Neo.VM.Types.Array;

namespace Neo.SmartContract.Native
{
    /// <summary>
    /// Represents an Oracle request in smart contracts.
    /// </summary>
    public class OracleRequest : IInteroperable
    {
        /// <summary>
        /// The original transaction that sent the related request.
        /// </summary>
        public UInt256 OriginalTxid;

        /// <summary>
        /// The maximum amount of GAS that can be used when executing response callback.
        /// </summary>
        public long GasForResponse;

        /// <summary>
        /// The url of the request.
        /// </summary>
        public string Url;

        /// <summary>
        /// The filter for the response.
        /// Can be null.
        /// </summary>
        public string? Filter;

        /// <summary>
        /// The hash of the callback contract.
        /// </summary>
        public UInt160 CallbackContract;

        /// <summary>
        /// The name of the callback method.
        /// </summary>
        public string CallbackMethod;

        /// <summary>
        /// The user-defined object that will be passed to the callback.
        /// </summary>
        public byte[] UserData;

        public void FromStackItem(StackItem stackItem)
        {
            Array array = (Array)stackItem;
            OriginalTxid = new UInt256(array[0].GetSpan());
            GasForResponse = (long)array[1].GetInteger();
            // must contain a valid Url
            Url = array[2].NotNull().GetString().NotNull();
            // Filter can be null based on ToStackItem
            Filter = array[3].GetString();
            CallbackContract = new UInt160(array[4].GetSpan());
            // must contain a valid callback method
            CallbackMethod = array[5].NotNull().GetString().NotNull();
            UserData = array[6].GetSpan().ToArray();
        }

        public StackItem ToStackItem(ReferenceCounter? referenceCounter)
        {
            return new Array(referenceCounter)
            {
                OriginalTxid.ToArray(),
                GasForResponse,
                Url,
                Filter ?? StackItem.Null,
                CallbackContract.ToArray(),
                CallbackMethod,
                UserData
            };
        }
    }
}
