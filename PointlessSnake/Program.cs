using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class SnakeGame
    {
        static Board board;

        static void Main()
        {
            board = new Board();

            Console.WriteLine("Type 'down', 'left', 'right', or 'up' to maneuver the snake at whatever pace you find appropriate.");

            while (!board.IsGameLost && !board.IsGameWon)
            {
                board.PrintBoard();
                Console.WriteLine("What's your next move?");
                string playerInput = Console.ReadLine();
                HandleInput(playerInput);
            }

            if (board.IsGameLost)
            {
                Console.WriteLine("You lose!");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("You win!");
                Console.ReadKey();
            }
        }

        private static void HandleInput(string input)
        {
            switch (input)
            {
                case "down":
                    board.HandleSnakeMovement(Board.MovementDirection.Down);
                    return;
                case "up":
                    board.HandleSnakeMovement(Board.MovementDirection.Up);
                    return;
                case "right":
                    board.HandleSnakeMovement(Board.MovementDirection.Right);
                    return;
                case "left":
                    board.HandleSnakeMovement(Board.MovementDirection.Left);
                    return;
                default:
                    break;
            }
        }
    }

    class Board
    {
        // References.
        string[][] gameBoard;
        Snake snake;
        Random random;

        // State.
        bool isGameLost = false;
        bool isGameWon = false;
        bool shouldSpawnFood;
        int[] foodPosition;

        // Public accessors.
        public bool IsGameLost { get { return isGameLost; } }
        public bool IsGameWon { get { return isGameWon; } }

        // Consts for rendering.
        const string foodSymbol = "O";
        const string floorSymbol = "x";
        const string snakeSymbol = "S";

        public enum MovementDirection { Up, Down, Left, Right };

        // Initialization.
        public Board()
        {
            random = new Random();
            GenerateEmptyBoard();
            CreateBabySnake();
            GenerateNewFoodPosition();
            PlaceFood();
        }

        #region Board manipulation
        // Generates an 8x8 array of floor tiles.
        private void GenerateEmptyBoard()
        {
            gameBoard = new string[8][];

            for (int y = 0; y < gameBoard.Length; y++)
            {
                string[] row = new string[8];
                for (int x = 0; x < gameBoard.Length; x++)
                {
                    row[x] = floorSymbol;
                }
                gameBoard[y] = row;
            }
        }

        // Used on startup.
        private void CreateBabySnake()
        {
            snake = new Snake();
            for (int y = 1; y < 4; y++)
            {
                int x = 1;
                snake.AddHead(x, y);
            }

            DrawSnake(snake.SnakePieces);
        }

        // Renders snake onto board.
        private void DrawSnake(LinkedList<SnakePiece> snakePieces)
        {
            LinkedListNode<SnakePiece> currentPiece = snakePieces.First;
            while (currentPiece != null)
            {
                int xCoordinate = currentPiece.Value.Coordinates[0];
                int yCoordinate = currentPiece.Value.Coordinates[1];

                gameBoard[yCoordinate][xCoordinate] = snakeSymbol;
                currentPiece = currentPiece.Next;
            }
        }

        // Chooses and assigns a new food position based on available spaces.
        // If no available spaces, player wins.
        int[] GenerateNewFoodPosition()
        {
            List<int[]> validFoodSpawns = new List<int[]>();

            for (int y = 0; y < gameBoard.Length; y++)
            {
                string[] row = gameBoard[y];
                for (int x = 0; x < row.Length; x++)
                {
                    if (row[x] != snakeSymbol)
                    {
                        validFoodSpawns.Add(new int[2] { x, y });
                    }
                }
            }

            if (validFoodSpawns.Count() == 0)
            {
                isGameWon = true;
                return new int[2] { 0, 0 };
            }

            int randomIndex = random.Next(validFoodSpawns.Count);
            foodPosition = validFoodSpawns[randomIndex];

            return foodPosition;
        }

        // Renders food onto board based on position.
        void PlaceFood()
        {
            int foodX = foodPosition[0];
            int foodY = foodPosition[1];

            gameBoard[foodY][foodX] = foodSymbol;
        }
        #endregion

        #region Game loop
        public void HandleSnakeMovement(MovementDirection direction)
        {
            int[] currentHeadPosition = snake.GetHeadPosition();
            int newX = currentHeadPosition[0];
            int newY = currentHeadPosition[1];

            // Updates head position; if out of bounds, player loses.
            switch (direction)
            {
                case MovementDirection.Up:
                    newY--;
                    if (newY < 0)
                    {
                        isGameLost = true;
                        return;
                    }
                    break;
                case MovementDirection.Down:
                    newY++;
                    if (newY >= gameBoard.Length)
                    {
                        isGameLost = true;
                        return;
                    }
                    break;
                case MovementDirection.Left:
                    newX--;
                    if (newX < 0)
                    {
                        isGameLost = true;
                        return;
                    }
                    break;
                case MovementDirection.Right:
                    newX++;
                    if (newX >= gameBoard.Length)
                    {
                        isGameLost = true;
                        return;
                    }
                    break;
                default:
                    break;
            }

            UpdateBoardAndState(newX, newY);
        }

        // Updates gamestate based on new position.
        // If snake touches own body, player loses.
        // If snake touches food, player grows and new food is generated.
        // Else, snake moves in whatever direction
        // If game is not over, board is generated from current gamestate.
        void UpdateBoardAndState(int newX, int newY)
        {
            string charAtNewHeadPosition = gameBoard[newY][newX];

            switch (charAtNewHeadPosition)
            {
                case snakeSymbol:
                    isGameLost = true;
                    return;
                case foodSymbol:
                    shouldSpawnFood = true;
                    break;
                case floorSymbol:
                    snake.RemoveTail();
                    break;
                default:
                    break;
            }

            snake.AddHead(newX, newY);
            GenerateEmptyBoard();
            DrawSnake(snake.SnakePieces);

            if (shouldSpawnFood)
            {
                foodPosition = GenerateNewFoodPosition();
                shouldSpawnFood = false;
            }

            PlaceFood();
        }
        #endregion

        #region Rendering
        // Renders board in console.
        public void PrintBoard()
        {
            for (int y = 0; y < gameBoard.Length; y++)
            {
                string row = "";
                for (int x = 0; x < gameBoard.Length; x++)
                {
                    row += gameBoard[y][x] + " ";
                }
                Console.WriteLine(row);
            }
        }
        #endregion
    }

    // Class to hold data related to current snake size/position.
    class Snake
    {
        LinkedList<SnakePiece> snakePieces;
        public LinkedList<SnakePiece> SnakePieces { get { return snakePieces; } }

        public Snake()
        {
            snakePieces = new LinkedList<SnakePiece>();
        }

        public void AddHead(int xCoordinate, int yCoordinate)
        {
            SnakePiece newHead = new SnakePiece(xCoordinate, yCoordinate);

            snakePieces.AddFirst(newHead);
        }

        public int[] GetHeadPosition()
        {
            return snakePieces.First().Coordinates;
        }

        public void RemoveTail()
        {
            snakePieces.RemoveLast();
        }

    }

    // Class for each individual segment of the snake.
    class SnakePiece
    {
        int[] coordinates;
        public int[] Coordinates { get { return coordinates; } }

        public SnakePiece(int x, int y)
        {
            coordinates = new int[2] { x, y };
        }
    }
}
