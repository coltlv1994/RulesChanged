# RulesChanged - 红色警戒2规则文件修改器
## What's new
1. 采用.NET 6和WPF编写，可运行在Windows 7 SP1及更新的系统上。
2. 现代的GUI，可支持HiDPI显示器。

## 为什么我想做这个
首先，我曾经是，而且至今仍然是红警2的玩家。这个游戏很老，画面很落后，但是都没关系。很多年以来我一直尝试自己修改规则文件，比如使用Sublime Text或者Notepad++这类文本编辑器；但这很不方便：首先你得知道你想改的是什么，然后再去反复搜索字符串的位置。这非常麻烦，谁试谁知道。
之前确实有人做过，比如“冷晓辉”。但这些采用MFC的编辑器已经是过时不用的技术，功能也较为有限。最重要的是，源代码无可查考，导致现在想做任何修改都不可能了。
我选择使用比较新的技术来编写，并将其开源；如果以后还有人想继续修改，他或者她就不用担心无从下手。

## 作者想说的一些七七八八
我虽然是个程序员，但是我是做ASIC C的开发的。绝大部分时候我的目标机器没有显示器，更不要说任何GUI，甚至命令行都不提供；能供我debug的只有log。在这个项目里你可能看到很多你不喜欢的很奇怪的实现，我目前能做的只是确保它“能用”，不会出现大面积的内存泄露。我也不是算法专家，所以不要对这个编辑器的效率有过高期待。如果你有任何建议或者想法，欢迎留言或者给我写邮件。
我的母语是汉语，但为了开发过程中的方便，我选择使用英语来做GUI。如果有任何人想做本地化工作，我很感谢，欢迎联络。

## 开发路线图
1. 能正确显示规则文件里所有的tag下面的项目
2. 让它“能用”，可以打开，读取，展示，修改和保存一个给定的规则文件。在这一步尚不能加入新的条目。
3. 可以给建筑，步兵，车辆，武器，弹头和弹道增加新的条目。
4. 修改某些属性的时候，制作一个下拉菜单，可以从菜单里选择所有可供选择的值。
5. 在第四步选择的时候，可以添加一个新的条目。
6. 无穷无尽的维护和debug……

## 其他感兴趣的工作
1. 把它移植到Windows XP上，大概要使用.Net Framework 4.5.
2. 让他同时支持中文和英文。

## 开发环境
Intel Core i9-9900K, 32GB RAM,
Windows 10 21H2, Visual Studio 2022,
.NET 6, WPF 

## 联系方式
colt.github@outlook.com

# RulesChanged - Red Alert 2 rules.ini/rulesmd.ini GUI editor

## What's new
1. Implemented in .NET 6 with WPF, runs smoothly on Windows 7 SP1 and above.
2. Modernized GUI, works better on HiDPI monitors.

## Why I want to do that
First of all, I was, and still am, a player of Red Alert 2. It is old; its graphics is out-dated. But it does not matter. For years, I am trying to modify the rule files (rules.ini and rulesmd.ini, for Red Alert 2 and Red Alert 2: Yuri's Revenge, respectively), using a number of editors, like Sublime Text or Notepad++, but for using that you need absolutely know what you are looking for, and do searches from time to time.
We do, have some implemented editor with GUI, but they are mostly implemented on deprecated techniques, and functions are limited. Most importantly, the source code was lost. It is impossible to make any further improvement.
I choose to implement this editor on most recent techniques, and choose to make it open-sourced. So if anyone ever has any interest on making changes, he/she can do that without worries.

## Some explanation from author
Although I am a programmer, but I am a rookie to Windows/GUI programming, and rookie to .NET and WPF; most of my previous work is focusing on C of ASIC systems, no display, no GUI, even no CLI, only logs for debugging. You may find many tricky implementations you don't like, well, what I can do by now, is to make sure the functionalities work as expected without too serious memory leak. Also, I am not an expert of algorithm, so don't expect much on the efficiency of this editor.  If you have any comments or idea, leave a message or send a email to me. 
I am a native Chinese speaker, but for convenience of development, I choose to use English. If anyone want to do any localization work, please contact me, I am appreciated.

## Roadmap
1. Can display all entries from every tags in rule file.
2. Make it a fully-operational editor, i.e. it can open, read, present, modify and write back a given rule file. You cannot add new entries by this step.
3. Can create new entry for buildings, infantries, vehicles, weapons, warheads and projectiles.
4. When modifying entry's attributes, for certain attributes, like "Owner", "Prerequisite" and "Primary", let user choose from a list with all possible options.
5. When choosing from step 4, make it possible to add new entry.
6. Endless road of maintain and debug...

## Other work might interest me
1. Backport this project to Windows XP, using .Net Framework 4.5.
2. Make it bi-lingual using Chinese and English.

## Development environment
Intel Core i9-9900K, 32GB RAM,
Windows 10 21H2, Visual Studio 2022,
.NET 6, WPF

## Contact me
Email: colt.github@outlook.com