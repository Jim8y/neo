// Copyright (C) 2015-2024 The Neo Project.
//
// MerkleTreeNode.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.Cryptography.MerkleTree
{
    public class MerkleTreeNode<T> where T : IArrayConvertible<T>, new()
    {
        public T Hash;
        public MerkleTreeNode<T> Parent;
        public MerkleTreeNode<T> LeftChild;
        public MerkleTreeNode<T> RightChild;

        public bool IsLeaf => LeftChild == null && RightChild == null;

        public bool IsRoot => Parent == null;
    }
}
