# **Archimedes** (ARC)

 Is a user defined structure based binary data engine intended to `load/save` and/or manipulate the proprietary *Rappelz Database (or `RDB`)* file format. Keeping data descriptions simple and the data itself in a boxed state, this engine is capable of impressive speeds.

## **Foreword**

This document will *(from this point on)* refer to the `Archimedes` engine as `ARC`. This document was created by and is maintained by the lead developer of `ARC` `iSmokeDrow` and is written with the express purpose to teach potential developers;

- The layout of typical rdb
- How structure definitions work
- How to create structure definitions
- How to create more complex definitions

**`ARC` is _NOT_ a general purpose binary parser! It is purposefully designed _only_ to parse the .rdb file format!!!**

## Structure Based?
Question: What does it mean to be **user defined structure based**?

Answer:

> It means that you, the user tell `ARC` the layout of a given `RDB` file. Based on that layout and some engine modifying flags `ARC` can then parse the contents of the `RDB`

## Structure definitions 

Structure definition files are simple `.lua` scripts that contain information regarding a structure. Like the; author, version, supported *(client)* epic and more. The structure also defines the header *(if applicable)* aswell as the body. By putting these pieces together we can describe every known `RDB` file. In the coming sections you will learn what properties an `ARC` structure definition can declare as-well as how to declare cells.

Lets start by opening your favorite text editor *(like notepad++)* and creating a new file called `StringResource.lua`  *(perhaps on your desktop)*. But before we continue, you should understand the layout of the typical `RDB` file.

### The devil is in the details!

`ARC` and `RDB2` aim to provide a much easier and more detailed `structure` file organization experience. So always be sure to utilize the info variables `ARC` provides. Reference the table below for more details.

|Name|Description|
|---|---|
|`name`*|Name of this structure *(e.g. StringResource)*|
|`database`|If provided `ARC` will use this value as the `database` name instead of the default value `Arcadia`|
|`file_name`|While not required, providing a value like *(db_string.rdb)* is strongly encouraged for ease of use during certain `IO` operations|
|`table_name`|While not required, providing a value like *(StringResource)* is strongly encouraged for ease of use during certain `SQL` operations|
|`version`|Optional, will default to `1.0.0.0`, provides more in-depth sorting in various `Structure` Select `GUI`
|`author`|While not required, providing your name or the name of the creator is strongly encouraged cause this stuff can be hard. **Don't be a wank, give a thank!**|
|`epic`|If defined can provide more in-depth sorting in various `Structure` Select `GUI`. Defined like. `epic = { 0 }` (for all), `epic = {0, 7.2}` (all -> 7.2), `epic = {7.2, 8.1}` (7.2 -> 8.1)
|`encoding`|`int` codepage of the desired language encoding.|
|`special_case`** |`SpecialCase` **`KEYWORD`** indicating `ARC` should behave in a _special_ way|
||

> **Note:** `*` denotes **required** variable!

> **Note:** `**` changes the core `IO` behavior'(s) of `ARC`

### **Describing a Structure**
---
The header (if applicable) and the body of any given `RDB` are described in the same way. Using `Moonsharp` we can interpret a `lua` `table` into dynamic `.net` objects. The only difference between declaring the header or body is only the name of the root `lua` table. *(e.g. header, cells)* Other than that the collection of tables inside of that root table *(the cell descriptions)* are declared in the same way.

A cell is declared by creating a table *( { } )* that consists of the following:

- name
- an `ArcType` `KEYWORD`
- any required variables and flags. 

Consider the basic example below.

```lua
require `BitHelper`

cells = {
--      { name, ArcType KEYWORD, var/flag'(s) }
        { "val0", INT32, HIDDEN }, -- Setting a single flag can be done by the CellFlags keyword
        -- Setting multiple flags requires the use of BitHelper
        { "val1", INT32, SetFlags({HIDDEN, SQILIGNORE}) }, -- Provide a table of the CellFlags keywords to be set
        { "val2", INT32 }
    }
}
```

Now that you are more familiar with the way you can use `ARC` to describe any given `RDB`'(s) data. Make sure your `StringResource.lua` is filled out like the example below. *(Then we can move on to defining some actual header/body cells.)*

```lua
require 'BitHelper'

name = "StringResource"
file_name = "db_string.rdb"
table_name = "StringResource"
version = "0.1.0.0"
author = "iSmokeDrow"
epic = { 0 }
encoding = 1252
```

### **Typical RDB Layout**
---
Normally any given `RDB` will consist of a generic header and a set *(in our case, dynamic, user provided)* structure of data *(or body)* cells. Reference the table below to see the structure of the header data.

**Traditional Header**

|Name|Type|Length|
|---|---|---|
|CreateDate|`string`|8|
|Comments/Signature|`byte`|120
|RowCount|`int`|4
||

> **Note**: some rdb like 9.4+ `db_item` require the use of a special file type `.ref` which uses a user defined header!

#### **How is the header defined?**

In our `StringResource.lua` we do not need to define a header, as this `RDB` uses the traditional header.

However, if the given file is a `.ref` then we must provide a header for `ARC`. Consider the example below.

```lua
header = {
	{ "rows", INT32, ROWCOUNT }, -- ROWCOUNT identifies the engine will loop on the `int` value this cell
	{ "strLen", INT32 } -- Holds the length of cells.reference 
}
```

### **Cells** *(and how they work)*

While Header `Cells` describe data that exists in the header area of the file. Body `Cells` describe the data that exists immediately after the header section of a given `RDB` file. This data can be varied and complex, so we must concisely describe *(to the best of our ability)* the data contained within. By creating a `Cell` *(or cell description)* you can describe the data in a given area of the `RDB` file being parsed. By attaching `CellFlags` you can alter the way the data being parsed is handled, some `ArcType` however require extra information to be processed. *(see `ArcType`.`SKIP`)*

#### **CellFlags**
---

|Name|Description|
|---|---|
|`HIDDEN`|This cell will be hidden from the row grid of the `RDB2` tab.|
|`SQLIGNORE`|This cell will be ignored in all `SQL` related operations.|
|`RDBIGNORE`|This cell will be ignored in all `RDB` `IO` operations.|
|`LOOPCOUNTER`|This cells value will be called against certain `IO` operations.|
|`ROWCOUNT`|This cells value will be used by `ARC` as the read loop count.|
||

> **Note:** all **`ArcType`**'s can bear many combinations of given `CellFlags`. Refer to the below table of currently implemented flags.

#### Setting `Cell` Flags
---
**Single Flag**

Declare its `CellFlags` **`KEYWORD`** as the last value in the `Cell` description table.

**Multiple Flags**

Require the use of the `BitHelper` function, which itself requires the structure file to declare a `require` statement. See the example below.

```lua
require 'BitHelper'
```

Once the `BitHelper` module has been required, now you can use the `SetFlags` function *(or any other declared inside* `BitHelper.lua`*)*

> **Note:** Required modules are loaded from the `.\Modules` directory.

#### **ArcTypes**
---

> **Note**: Cell `ArcType KEYWORD`'(s) are always in full **`CAPS`**!!!

|Type `KEYWORD`|Primary Data Type|Secondary Data Type|Length|Description|Required Variables & Flags|
|---|---|---|---|---|---|
|`BYTE`|`byte`|`TYPE_BYTE`|1|Single byte of data|None|
|`BYTE_ARRAY`|`byte[]`|`TYPE_BYTE_ARRAY`|?|Variable length byte array|`Length` of the array
|`BIT_VECTOR`|`int`|`TYPE_BIT_VECTOR`|4|`int` value containing *(32)* bits indexed to store multiple `bool` flags|None|
|`BIT_FROM_VECTOR`|`byte`|`TYPE_BIT_FROM_VECTOR`|0|Not actually read from the `RDB` stream but instead indexed from the dependent cells `BIT_VECTOR` value.|`Dependency` cell name, Zero based `Offset` of the target bit|
|`INT16/SHORT`|`short`|`TYPE_INT16/SHORT`|2|`signed` 2 byte `integer` value|None|
|`UINT16/USHORT`|`ushort`|`TYPE_UINT16/USHORT`|2|`unsigned` 2 byte `integer` value|None|
|`ENCODED_INT32`|`int`|`TYPE_ENCODED_INT32`|4|Encoded `signed` 4 byte `integer` value|None|
|`COPY_INT32`|`int`|`TYPE_COPY_INT32`|4|Read as an int, written by copying the value of the dependent cell|`Dependency` cell name.|
|`INT32/INT`|`int`|`TYPE_INT32/INT`|4|`signed` 4 byte `integer` value|None|
|`UINT32/UINT`|`uint`|`TYPE_UINT32/USHORT`|4|`unsigned` 4 byte `integer` value|None|
|`INT64/LONG`|`long`|`TYPE_INT64/LONG`|8|`signed` 4 byte `integer` values|None|
|`FLOAT/FLOAT32/SINGLE`|`float`|`TYPE_FLOAT/FLOAT32/SINGLE`|4|Single precision floating point value|None|
|`FLOAT64/DOUBLE`|`double`|`TYPE_DOUBLE`|8|Double precision floating point value|None|
|`DECIMAL`|`int`|`TYPE_DECIMAL`|4|`int` value with math applied for conversion|None|
|`DATETIME`|`DateTime`|`TYPE_DATETIME`|4|`int` value storing the seconds since epoch `1970.01.01`|None|
|`DATESTRING`|`string`|`TYPE_DATESTRING`|8|string representing a date *(formatted: `yyyyMMdd`)*|None|
|`SID`|`int`|`TYPE_SID`|0|Not read from the `RDB` stream. Incremented once per read loop|None|
|`STRING`|`string`|`TYPE_STRING`|?|String value whose length is provided.|`Length` of the string|
|`STRING_LEN`|`int`|`TYPE_STRING_LEN`|4|`Length` of a parent `CellBase`|`SetFlags({ HIDDEN, SQLIGNORE })` recommended|
|`STRING_BY_LEN`|`string`|`TYPE_STRING_BY_LEN`|?|`string` value whose `length` is provided by the `dependent` cells value|`dependency` cell name|
|`SKIP`|`byte[]`|`TYPE_SKIP`|0|Not read from the `RDB` stream. Instead if `read` loop, the stream is advanced by the given `Length`. If `write` loop, a blank array of the given `Length` is written to the stream.|`Length` of the skip.
||

#### **Defining the `Cells` Table**
---
Considering the cell type table above and the `CellFlags` table above it. Lets define the body of our `db_string.rdb` target file.

```lua
require 'BitHelper'

name = "StringResource"
file_name = "db_string.rdb"
table_name = "StringResource"
version = "0.1.0.0"
author = "iSmokeDrow"
epic = { 0 }
encoding = 1252

cells = 
{
	{ "name_len", STRING_LEN, SetFlags({ HIDDEN, SQLIGNORE}) },
	{ "value_len", STRING_LEN, SetFlags({ HIDDEN, SQLIGNORE}) },
	{ "name", STRING_BY_LEN, "name_len" },
	{ "value", STRING_BY_LEN, "value_len" },
	{ "code", INT32 },
	{ "group_id", INT32 },
	{ "blank", SKIP, 16, SetFlags({ HIDDEN, SQLIGNORE}) } 
}
```

### **Complex Structure Definitions**
---
`RDB` file structures vary widely depending on the data stored and any special effort `GALA` went through to obscure that data from us. *(see* Example `A`*)* Certain techniques to save memory/disk space are also implemented. Like storing multiple `bool` values in a single `int` *(see* Examples `B, C` and `D`*)* 

Some `RDB` however, require even more special considerations to the way their structure is declared. As-well as which `SpecialCase`, `CellFlags` and/or other special properties that must be used.

#### **MonsterResource / ItemResource / QuestResource**
---
**Example `A`**
```lua
    {"id", ENCODED_INT32}
```
> **Note:** The above example is from `MonsterResource72.lua`

**Example `B`**
```lua
	{"limit_bits", BIT_VECTOR, SetFlags({ HIDDEN, SQLIGNORE }) },
	{"limit_deva", BIT_FROM_VECTOR, "limit_bits", 2},
	{"limit_asura", BIT_FROM_VECTOR, "limit_bits", 3},
	{"limit_gaia", BIT_FROM_VECTOR, "limit_bits", 4},
	{"limit_fighter", BIT_FROM_VECTOR, "limit_bits", 5},
	{"limit_hunter", BIT_FROM_VECTOR, "limit_bits", 6},
	{"limit_magician", BIT_FROM_VECTOR, "limit_bits", 7},
	{"limit_summoner", BIT_FROM_VECTOR, "limit_bits", 8},
```
**Example `C`**
```lua
	{ "time_limit_type_bits", BIT_VECTOR, SetFlags({ HIDDEN, SQLIGNORE }) },
	{ "time_limit_type", BIT_FROM_VECTOR, "time_limit_type_bits", 32},
```

**Example `D`**
```lua
	{ "or_flag_bits", BIT_VECTOR, SetFlags({ HIDDEN, SQLIGNORE }) },
	{ "or_flag", BIT_FROM_VECTOR, "or_flag_bits", 1}
```
> **Note:** The above example is from `QuestResource72.lua`

> **Note:** `ENCODED_INT32`, `BIT_VECTOR` & `BIT_FROM_VECTOR` are all documented in the `ArcType` table above.

#### **ItemReference**
---
As of epic `9.4` and onward. Gala seperated the `db_item.rdb` into two files.

- `db_item.rdb`
- `db_item.ref`

The `db_item.ref` file is unique in that it does not use the `Traditional Header` as detailed above. But instead requires a `Defined Header`.

In addition to this fact, this particular file also uses two rare `ArcType`

- `SID` - This cell value is not read but simply incremented per read loop
- `STRING_BY_HEADER_REF` - This cells length is taken from the dependent cell *(in the Header row)*

Refer to the below example of a `ItemRef.lua`

```lua
name = "ItemReference"
file_name = "db_item.ref"
table_name = "ItemReferenceResource"
version = "0.1.0.0"
author = "Gangor, Glandu2, InkDevil"
epic = { 9.5, 99 }
encoding = 1252

header = {
	{ "rows", INT32, ROWCOUNT }
	{ "strLen", INT32 }
}

cells = {
	{ "id", SID },
	{ "reference", STRING_BY_HEADER_REF, "strLen" } 
}
```

##### **Importing from SQL**

While not wholely different from the way a typical `RDB` is imported from an `SQL` table. The data being loaded into this particular `rdb` structure is different in that it must be selected from an `SQL View` Ensure you have created appropriate views for your particular client epic. Consider downloading the example for epic `9.4+` created by `Gangor` of `epvp` [here](https://www.elitepvpers.com/forum/attachment.php?attachmentid=275069&d=1509466332). 

Once the views have been created simply set the structures `table_name` as the name of the view and make sure to set the `select_statement` appropriately for special data needs.

#### **SkillResource**
---
The `db_skill.rdb` data structure is not unique compared to other `RDB`. However, it does combine data from two seperate `SQL` tables into one file. While `ARC` can generate a generic `select statement` based on the user provided layout in most cases, it cannot predict that data will need to be selected from multiple `SQL` tables.

It is for this purpose that `ARC` exposes the `select_statement` property. With this, we can specify a more specified query which `ARC` can then use to `select` its data from the `SQL` table. Refer to the example below.

```lua
-- Use DISCTINT keyword to avoid duplicates left by gala
select_statement = "SELECT DISTINCT [id],[text_id],[is_valid],[elemental],[is_passive],[is_physical_act],[is_harmful],[is_need_target],[is_corpse],[is_toggle],[casting_type],[casting_level],[toggle_group],[cast_range],[valid_range],[cost_hp],[cost_hp_per_skl],[cost_mp],[cost_mp_per_skl],[cost_mp_per_enhance],[cost_hp_per],[cost_hp_per_skl_per],[cost_mp_per],[cost_mp_per_skl_per],[cost_havoc],[cost_havoc_per_skl],[cost_energy],[cost_energy_per_skl],[cost_exp],[cost_exp_per_enhance],[cost_jp],[cost_jp_per_enhance],[cost_item],[cost_item_count],[cost_item_count_per_skl],[need_level],[need_hp],[need_mp],[need_havoc],[need_havoc_burst],[need_state_id],[need_state_level],[need_state_exhaust],[vf_one_hand_sword],[vf_two_hand_sword],[vf_double_sword],[vf_dagger],[vf_double_dagger],[vf_spear],[vf_axe],[vf_one_hand_axe],[vf_double_axe],[vf_one_hand_mace],[vf_two_hand_mace],[vf_lightbow],[vf_heavybow],[vf_crossbow],[vf_one_hand_staff],[vf_two_hand_staff],[vf_shield_only],[vf_is_not_need_weapon],[delay_cast],[delay_cast_per_skl],[delay_cast_mode_per_enhance],[delay_common],[delay_cooltime],[delay_cooltime_mode_per_enhance],[cool_time_group_id],[uf_self],[uf_party],[uf_guild],[uf_neutral],[uf_purple],[uf_enemy],[tf_avatar],[tf_summon],[tf_monster],[skill_lvup_limit],[target],[effect_type],[state_id],[state_level_base],[state_level_per_skl],[state_level_per_enhance],[state_second],[state_second_per_level],[state_second_per_enhance],[state_type],[probability_on_hit],[probability_inc_by_slv],[hit_bonus],[hit_bonus_per_enhance],[percentage],[hate_mod],[hate_basic],[hate_per_skl],[hate_per_enhance],[critical_bonus],[critical_bonus_per_skl],[var1],[var2],[var3],[var4],[var5],[var6],[var7],[var8],[var9],[var10],[var11],[var12],[var13],[var14],[var15],[var16],[var17],[var18],[var19],[var20],[jp_01],[jp_02],[jp_03],[jp_04],[jp_05],[jp_06],[jp_07],[jp_08],[jp_09],[jp_10],[jp_11],[jp_12],[jp_13],[jp_14],[jp_15],[jp_16],[jp_17],[jp_18],[jp_19],[jp_20],[jp_21],[jp_22],[jp_23],[jp_24],[jp_25],[jp_26],[jp_27],[jp_28],[jp_29],[jp_30],[jp_31],[jp_32],[jp_33],[jp_34],[jp_35],[jp_36],[jp_37],[jp_38],[jp_39],[jp_40],[jp_41],[jp_42],[jp_43],[jp_44],[jp_45],[jp_46],[jp_47],[jp_48],[jp_49],[jp_50],[desc_id],[tooltip_id],[icon_id],[icon_file_name],[is_projectile],[projectile_speed],[projectile_acceleration] FROM dbo.SkillResource s LEFT JOIN dbo.SkillJpResource sj ON sj.skill_id = s.id"
```

> **Note:** The example above is from `SkillResource72.lua`

> **Note:** `ARC` indexes data in a row by its column name. The order of the columns selected is likely _insignificant_.

#### **SkillTree**
---
The `db_skilltree.rdb` data structure is one of the most unique `RDB` structures in use by `ARC` as it uses the rare `special_case` property. 

The `IO` process for this file is unique in that unlike a typical `RDB` which has a structure that is read as many times as the `RowCount` that exists in the header. This `RDB` has an initial `RowCount` but also a `SubRowCount`. So instead of a single `read` loop, we have to iterate the base loop and then a secondary loop on the `SubRowCount` *(a* **`DOUBLELOOP`***)*

Considering the above information, we must inform `ARC` that we need this `DOUBLELOOP` style read loop. We can do this by simply declaring the `special_case` property. Reference the example below.

```lua
name = "SkillTree"
file_name = "db_skilltree.rdb"
table_name = "SkillTreeResource"
version = "0.1.0.0"
author = "Glandu2"
epic = { 9.1 }

special_case = DOUBLELOOP

cells = {
	{"skill_tree_id", INT32, LOOPCOUNTER},
	{"skill_id", INT32},
	{"skill_group_id", INT32},
	{"min_skill_lv", INT32},
	{"max_skill_lv", INT32},
	{"lv", INT32},
	{"job_lv", INT32},
	{"jp_ratio", TYPE_FLOAT32},
	{"need_skill_id_1", INT32},
	{"need_skill_id_2", INT32},
	{"need_skill_id_3", INT32},
	{"need_skill_lv_1", INT32},
	{"need_skill_lv_2", INT32},
	{"need_skill_lv_3", INT32},
	{"cenhance_min", INT32},
	{"cenhance_max", INT32}
}
```

> **Note:** The example above is from `SkillResource91.lua`

> **Note:** As of the writing of this document there is only one known `SpecialCase`, that is `DOUBLELOOP`