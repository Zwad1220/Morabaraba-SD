const games = [
  {
    title: "Morabaraba",
    description: "The traditional African strategy game of 'Cows'. Outsmart your opponent and capture their herd.",
    image: "Morabaraba_picture.png",
    rules: `How to play: 

1. THE PLACEMENT PHASE
Each player starts with 12 pieces making 24 pieces in total (traditionally called "cows"). In this phase, each player will take turns placing one cow onto any empty node of their choice onto the board (click an empty circle). The goal during this phase is to position your cows to form "mills" while blocking your opponent from doing the same.

2. FORMING A MILL
A Mill is formed by placing three of your cows in a straight line; whether horizontally, vertically, or diagonally. Forming a mill is the primary objective and can be done during any phase to gain an advantage over your opponent. Doing so allows you to weaken your opponent's forces as seen in the following rule.

3. SHOOTING A COW
When you form a mill, you will be permitted to "shoot" (remove) one of your opponent's cows from the board by clicking on it. Cows that may be removed will be highlighted pink.

Note: You cannot shoot a cow that is currently part of a mill unless all of your opponent's cows are already in mills.

4. THE MOVEMENT PHASE
Once all 12 cows have been placed on the board, the game shifts to movement. Players take turns moving one cow to an adjacent empty spot along the lines of the board. You continue to try and form mills to remove your opponent's pieces.
Clicking a piece once will highlight it green, indicating it is selected. Click on an adjacent empty spot to move the cow there.

Breaking the Mill: During the movement phase, you may move a cow out of an existing mill or into a new one.

5. THE FLYING PHASE
When a player is reduced to only three cows, they automatically enter the "Flying Phase". At this point the player is no longer restricted to adjacent spots; they can "fly" their cows to any empty node on the board. Thus giving the losing player a "last stand" to force a draw or a comeback.

6. WINNING THE GAME
The win conditions are as follows:
-Reduction: Your opponent is reduced to only two cows (making it impossible for them to form a mill).
-Gridlock: Your opponent is unable to make any legal moves (all their pieces are blocked) the last player to successfully place or move a piece wins.
However, if either player is reduced to three cows, if no captures have been made within ten moves, or if both players are unable to form mills, the game is declared a draw.

Good luck, may the best strategist win!
    `,
    page: "game.html"
  }
];

// Selections via querySelector
const gameGrid = document.querySelector(".game-grid");
const modal = document.querySelector(".modal");
const modalTitle = document.querySelector(".modal-title");
const modalRules = document.querySelector(".modal-rules");
const closeModal = document.querySelector(".close-btn");
const startBtn = document.querySelector(".start-btn");

let destination = "";

function renderGames() {
  games.forEach(game => {
    const card = document.createElement("div");
    card.classList.add("game-card");

    card.innerHTML = `
      <img src="${game.image}" alt="${game.title}" class="game-image">
      <div class="game-info">
        <h2 class="game-title">${game.title}</h2>
        <p class="game-description">${game.description}</p>
        <button class="play-btn">Play Morabaraba</button>
      </div>
    `;

    // Handle Play Click
    card.querySelector(".play-btn").addEventListener("click", () => {
      modalTitle.textContent = game.title;
      modalRules.textContent = game.rules;
      destination = game.page;
      modal.classList.remove("hidden");
    });

    gameGrid.appendChild(card);
  });
}

// Modal closing logic
closeModal.addEventListener("click", () => {
  modal.classList.add("hidden");
});

// Outside click to close
modal.addEventListener("click", (e) => {
  if (e.target === modal) modal.classList.add("hidden");
});

// Start Game logic
startBtn.addEventListener("click", () => {
  if (destination) {
    window.location.href = destination;
  }
});

// Initialize
renderGames();