import 'package:flutter/material.dart';

class MainIndexView extends StatefulWidget {

  @override
  _MainIndexViewState createState() => _MainIndexViewState();
}

class _MainIndexViewState extends State<MainIndexView>{
  int _counter1 = 0;
  String _title = 'Lemon Demo';

  void _incrementCounter() {
    setState(() {
      _counter1++;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        title: Text(_title),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            const Text(
              'You have pushed the button this many times:',
            ),
            Text(
              '$_counter1',
              style: Theme.of(context).textTheme.headlineMedium,
            ),
            Text(
              '哈哈哈哈',
            ),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _incrementCounter,
        tooltip: 'Increment',
        child: const Icon(Icons.add),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}