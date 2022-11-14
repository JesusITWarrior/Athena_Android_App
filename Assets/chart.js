<html>
    <head>
        <!--Load the AJAX API-->
        <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
        <script type="text/javascript">

        // Load the Visualization API and the corechart package.
            google.charts.load('current', {'packages': ['corechart'] });

            // Set a callback to run when the Google Visualization API is loaded.
            //google.charts.setOnLoadCallback(drawChart);

            // Callback that creates and populates a data table,
            // instantiates the pie chart, passes in the data and
            // draws it.
            function testFunction(jsonData) {
            var text = document.getElementById("textItem");
            //var json = atob(jsonData);
            text.textContent = jsonData;
        }

            function drawAthenaChart(databaseData, chartType) {
                databaseData = databaseData.replace(/'/g, "\"");
            var text = document.getElementById("textItem");
            const obj = JSON.parse(databaseData);
            //text.textContent = obj[1].updatedTime;

            var dataArray = [
            ['Time', 'Temperature', {role: 'style'}]
            ];

            for (var i = 0; i < obj.length; i++) {
                dataArray.push([obj[i].updatedTime, obj[i].Temperature, (obj[i].DoorOpenStatus ? 'red' : 'green')]); //Fix this later
            }

            //text.textContent = dataArray[1][2];

            var data = google.visualization.arrayToDataTable(dataArray);

            var options = {
                title: 'Athena Chart',
            subtitle: 'Yep',
            width: 400,
            height: 400
                //legend: {position: 'none' },
                //colors: ['red','green']
            };

            // Instantiate and draw our chart, passing in some options.
            switch (chartType) {
                /*case 0:
            var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
            break;*/
                case 0:
            var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
            break;
            case 1:
            var chart = new google.visualization.BarChart(document.getElementById('chart_div'));
            break;
            case 2:
            var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
            break;
            case 3:
            var chart = new google.visualization.Table(document.getElementById('chart_div'));
            break;
            }

            chart.draw(data, options);
        }

            function drawChart(databaseData, chartType) {

            // Create the data table.
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Topping');
            data.addColumn('number', 'Slices');
            data.addRows([
            ['Mushrooms', 3],
            ['Onions', 1],
            ['Olives', 1],
            ['Zucchini', 1],
            ['Pepperoni', 2]
            ]);

            // Set chart options
            var options = {
                'title': 'How Much Pizza I Ate Last Night',
            'width': 400,
            'height': 300
            };

            // Instantiate and draw our chart, passing in some options.
            var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
            chart.draw(data, options);
        }
        </script>
    </head>

    <body>
        <p id="textItem"></p>

        <!--Div that will hold the pie chart-->
        <div id="chart_div"></div>
    </body>
</html>