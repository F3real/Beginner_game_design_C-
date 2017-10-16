using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <remarks>
    /// A number tile
    /// </remarks>
    class NumberTile
    {
        #region Fields

        // original length of each side of the tile
        int originalSideLength;

        // whether or not this tile is the correct number
        bool isCorrectNumber;

        //declare sound bank
        SoundBank soundBank;

        // drawing support
        Texture2D texture;
        Texture2D blinkingTexture;
        Rectangle drawRectangle;
        Rectangle sourceRectangle;

        //declare current texture
        Texture2D currentTexture;

        //set fields to track tile states
        bool isVisible = true;
        bool isShrinking;
        bool isBlinking ;
        bool buttonReleased;
        bool clickStarted;

        // blinking support
        const int TOTAL_BLINK_MILLISECONDS = 4000;
        int elapsedBlinkMilliseconds = 0;
        const int FRAME_BLINK_MILLISECONDS = 1000;
        int elapsedFrameMilliseconds = 0;

        //shrinking support
        const int TOTAL_SHRINK_MILLISECONDS = 4000;
        int elapsedShrinkMilliseconds=0;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentManager">the content manager</param>
        /// <param name="center">the center of the tile</param>
        /// <param name="sideLength">the side length for the tile</param>
        /// <param name="number">the number for the tile</param>
        /// <param name="correctNumber">the correct number</param>
        /// <param name="soundBank">the sound bank for playing cues</param>
        public NumberTile(ContentManager contentManager, Vector2 center, int sideLength,
            int number, int correctNumber, SoundBank soundBank)
        {
            // set original side length field
            this.originalSideLength = sideLength;

            // set sound bank field
            this.soundBank = soundBank; 

            // load content for the tile and create draw rectangle
            LoadContent(contentManager, number);
            drawRectangle = new Rectangle((int)center.X - sideLength / 2,(int)center.Y - sideLength / 2, sideLength, sideLength);

            // set isCorrectNumber flag
            isCorrectNumber = number == correctNumber;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the tile based on game time and mouse state
        /// </summary>
        /// <param name="gameTime">the current GameTime</param>
        /// <param name="mouse">the current mouse state</param>
        /// <return>true if the correct number was guessed, false otherwise</return>
        public bool Update(GameTime gameTime, MouseState mouse)
        {
            
            //check if tile is blinking
            if(isBlinking)
            {
                //update timers for blinking
                elapsedBlinkMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
                elapsedFrameMilliseconds += gameTime.ElapsedGameTime.Milliseconds;

                //check if tile is done blinking and return true
                if (elapsedBlinkMilliseconds >= TOTAL_BLINK_MILLISECONDS)
	            {
                    isVisible = false;
                    return true;
                }
		          
                //update frame
                if (FRAME_BLINK_MILLISECONDS-elapsedFrameMilliseconds <=0 )
                {
                    elapsedFrameMilliseconds = 0;
                    if (sourceRectangle.X == 0)
                    {
                        sourceRectangle.X += texture.Width / 2;
                    }
                    else
                    {
                        sourceRectangle.X = 0;
                    }
                }
            }

            //update shrinking time and calculate new sidelength
            else if (isShrinking)
            {
                elapsedShrinkMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
                float shrinkRatio = (TOTAL_SHRINK_MILLISECONDS - elapsedShrinkMilliseconds) / (float)TOTAL_SHRINK_MILLISECONDS;

                int newTileSideLength = (int)(originalSideLength * shrinkRatio);
                if (newTileSideLength > 0)
                {
                    this.drawRectangle.Width = newTileSideLength;
                    this.drawRectangle.Height = newTileSideLength;
                }
                else
                {
                    isVisible = false;
                }
            }
            else
            {
                // check for mouse over button
                if (drawRectangle.Contains(mouse.X, mouse.Y))
                {
                    // highlight button
                    sourceRectangle.X = texture.Width / 2;

                    // check for click started on button
                    if (mouse.LeftButton == ButtonState.Pressed && buttonReleased)
                    {
                        clickStarted = true;
                        buttonReleased = false;

                        // check if correct number is pressed and set blinking
                        if (isCorrectNumber)
                        {
                            isBlinking = true;
                            currentTexture = blinkingTexture;
                            sourceRectangle.X = 0;

                            //play sound for correct guess
                            soundBank.PlayCue("correctGuess");
                        }
                        else
                        {
                            isShrinking = true;

                            //play sound for incorrect guess
                            soundBank.PlayCue("incorrectGuess");
                        }
                    }
                    else if (mouse.LeftButton == ButtonState.Released)
                    {
                        buttonReleased = true;
                    }
                }
                else
                {
                    sourceRectangle.X = 0;

                    // no clicking on this button
                    clickStarted = false;
                    buttonReleased = false;
                }
            }


            return false;
        }

        /// <summary>
        /// Draws the number tile
        /// </summary>
        /// <param name="spriteBatch">the SpriteBatch to use for the drawing</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the tile
            if (isVisible)
            {
                spriteBatch.Draw(currentTexture, drawRectangle, sourceRectangle, Color.White);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the tile
        /// </summary>
        /// <param name="contentManager">the content manager</param>
        /// <param name="number">the tile number</param>
        private void LoadContent(ContentManager contentManager, int number)
        {
            // convert the number to a string
            string numberString = ConvertIntToString(number);

            // load content for the tile and set source rectangle
            texture = contentManager.Load<Texture2D>(numberString);
            drawRectangle = new Rectangle(0, 0, originalSideLength/2, originalSideLength/2);
            sourceRectangle = new Rectangle(0, 0, originalSideLength/2, originalSideLength/2);


            //load blinking rectangle and set currentTexture
            currentTexture = texture;
            blinkingTexture = contentManager.Load<Texture2D>("blinking" + numberString);

        }

        /// <summary>
        /// Converts an integer to a string for the corresponding number
        /// </summary>
        /// <param name="number">the integer to convert</param>
        /// <returns>the string for the corresponding number</returns>
        private String ConvertIntToString(int number)
        {
            switch (number)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                default:
                    throw new Exception("Unsupported number for number tile");
            }

        }

        #endregion
    }
}
