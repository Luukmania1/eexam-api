const express = require('express');
const pool = require('../db/connection');
const { verifyToken } = require('../middleware/auth');

const router = express.Router();

// Create exam
router.post('/exam', verifyToken, async (req, res) => {
  try {
    const { title, description, duration } = req.body;

    if (!title || !duration) {
      return res.status(400).json({ error: 'Title and duration required' });
    }

    const result = await pool.query(
      'INSERT INTO exams (title, description, duration) VALUES ($1, $2, $3) RETURNING id, title, description, duration',
      [title, description || '', duration]
    );

    res.status(201).json({
      message: 'Exam created successfully',
      exam: result.rows[0]
    });
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to create exam' });
  }
});

// Add question to exam
router.post('/question', verifyToken, async (req, res) => {
  try {
    const { examId, text, optionA, optionB, optionC, optionD, correctAnswer } = req.body;

    if (!examId || !text || !optionA || !optionB || !optionC || !optionD || !correctAnswer) {
      return res.status(400).json({ error: 'All fields required' });
    }

    if (!['A', 'B', 'C', 'D'].includes(correctAnswer)) {
      return res.status(400).json({ error: 'Correct answer must be A, B, C, or D' });
    }

    const result = await pool.query(
      'INSERT INTO questions (exam_id, text, option_a, option_b, option_c, option_d, correct_answer) VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING id, text, correct_answer',
      [examId, text, optionA, optionB, optionC, optionD, correctAnswer]
    );

    res.status(201).json({
      message: 'Question added successfully',
      question: result.rows[0]
    });
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to add question' });
  }
});

// Get all results (admin)
router.get('/results', verifyToken, async (req, res) => {
  try {
    const result = await pool.query(
      `SELECT r.id, u.username, e.title, r.score, r.total_questions, r.created_at,
              ROUND((r.score::float / r.total_questions * 100)::numeric, 2) as percentage
       FROM results r
       JOIN users u ON r.user_id = u.id
       JOIN exams e ON r.exam_id = e.id
       ORDER BY r.created_at DESC`
    );

    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch results' });
  }
});

module.exports = router;
