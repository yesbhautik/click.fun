const mongoose = require('mongoose');

const clickDataSchema = new mongoose.Schema({
    username: {
        type: String,
        required: true,
        index: true,
        ref: 'User'
    },
    mouseClicks: {
        type: Number,
        required: true,
        min: 0,
        default: 0
    },
    keyboardPresses: {
        type: Number,
        required: true,
        min: 0,
        default: 0
    },
    timestamp: {
        type: Date,
        default: Date.now,
        index: true
    }
}, {
    collection: 'clickdata',
    timestamps: true
});

// Create compound index for efficient querying
clickDataSchema.index({ username: 1, timestamp: -1 });

// Add method to get stats for last 24 hours
clickDataSchema.statics.getLast24HourStats = async function(username) {
    const twentyFourHoursAgo = new Date(Date.now() - 24 * 60 * 60 * 1000);
    
    const stats = await this.aggregate([
        {
            $match: {
                username,
                timestamp: { $gte: twentyFourHoursAgo }
            }
        },
        {
            $group: {
                _id: null,
                totalClicks: { $sum: '$mouseClicks' },
                totalPresses: { $sum: '$keyboardPresses' },
                recordCount: { $sum: 1 }
            }
        }
    ]);

    return stats[0] || {
        totalClicks: 0,
        totalPresses: 0,
        recordCount: 0
    };
};

module.exports = mongoose.model('ClickData', clickDataSchema); 