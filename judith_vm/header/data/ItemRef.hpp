#pragma once

#include "root.hpp"

struct ItemRef {
    static constexpr size_t TYPE_INTERNAL = 0;
    static constexpr size_t TYPE_NATIVE = 1;
    static constexpr size_t TYPE_EXTERNAL = 2;

    /// <summary>
    /// The kind of reference this is.
    /// </summary>
    size_t refType;

    ItemRef(size_t refType) : refType(refType) {}
    virtual ~ItemRef() {}
};

struct InternalRef : public ItemRef {
    /// <summary>
    /// The block the item is located in.
    /// </summary>
    size_t block;
    /// <summary>
    /// The index of said item inside the table for this kind of item.
    /// </summary>
    size_t index;

    InternalRef (size_t block, size_t index)
        : ItemRef(ItemRef::TYPE_INTERNAL), block(block), index(index)
    {}
};

struct NativeRef : public ItemRef {
    /// <summary>
    /// The index of this item inside the table for this kind of item in the
    /// native assembly.
    /// </summary>
    size_t index;

    NativeRef (size_t index) : ItemRef(ItemRef::TYPE_NATIVE), index(index) {}
};

struct ExternalRef : public ItemRef {
    // TODO: ASSEMBLY NAME????
    /// <summary>
    /// The index in the name table containing this item's block's name.
    /// </summary>
    size_t blockNameIndex;
    /// <summary>
    /// The index in the name table containing this item's name.
    /// </summary>
    size_t itemNameIndex;

    ExternalRef (size_t blockNameIndex, size_t itemNameIndex)
        : ItemRef(ItemRef::TYPE_EXTERNAL),
        blockNameIndex(blockNameIndex),
        itemNameIndex(itemNameIndex)
    {}
};